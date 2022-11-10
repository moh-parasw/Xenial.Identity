using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Xenial.Identity.Infrastructure.Localization;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Models;

using Xenial.Identity.Xpo.Storage.Models;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using DevExpress.Xpo.Metadata;
using Xenial.AspNetIdentity.Xpo.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Xenial.Identity.Data;
using Duende.IdentityServer;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Xenial.Identity;

public class MyAppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<XenialIdentityUser>
{
    public MyAppUserClaimsPrincipalFactory(UserManager<XenialIdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(XenialIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        //identity.AddClaims(user.GetAdditionalClaims());
        return identity;
    }
}

public sealed class UpdateDatabaseBackgroundService : IHostedService
{
    private readonly DatabaseUpdateHandler handler;

    public UpdateDatabaseBackgroundService(DatabaseUpdateHandler handler)
        => this.handler = handler;

    public Task StartAsync(CancellationToken cancellationToken)
        => handler.UpdateDatabase();

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class DatabaseUpdateHandler
{
    public const string AdminRoleName = "Administrator";
    public const string AdminUserName = "admin@admin.com";
    public const string AdminPassword = "!Admin321";
    private bool IsDatabaseUpToDate { get; set; }

    private readonly IServiceProvider provider;
    private readonly IConfiguration configuration;

    public DatabaseUpdateHandler(IServiceProvider provider, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.provider = provider;
    }

    public async Task UpdateDatabase()
    {
        if (IsDatabaseUpToDate)
        {
            return;
        }
        try
        {
            using (var scope = provider.CreateScope())
            using (var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>())
            {
                await UpdateDatabase(uow, scope.ServiceProvider, configuration);
            }
        }
        finally
        {
            IsDatabaseUpToDate = true;
        }
    }

    private static async Task SeedDatabase(
        UnitOfWork unitOfWork,
        IServiceProvider provider,
        bool createAdminUser = false
    )
    {
        if (unitOfWork.FindObject<XpoThemeSettings>(null) is null)
        {
            unitOfWork.Save(new XpoThemeSettings(unitOfWork));
            unitOfWork.CommitChanges();
        }
        if (unitOfWork.FindObject<XpoApplicationSettings>(null) is null)
        {
            unitOfWork.Save(new XpoApplicationSettings(unitOfWork));
            unitOfWork.CommitChanges();
        }


        var idsScope = await unitOfWork.Query<XpoApiScope>().FirstOrDefaultAsync(m => m.Name == IdentityServerConstants.LocalApi.ScopeName);
        if (idsScope is null)
        {
            idsScope = new XpoApiScope(unitOfWork)
            {
                Name = IdentityServerConstants.LocalApi.ScopeName,
                Enabled = true,
            };

            await unitOfWork.SaveAsync(idsScope);
            await unitOfWork.CommitChangesAsync();
        }

        var idsResource = await unitOfWork.Query<XpoApiResource>().FirstOrDefaultAsync(m => m.Name == IdentityServerConstants.LocalApi.ScopeName);
        if (idsResource is null)
        {
            idsResource = new XpoApiResource(unitOfWork)
            {
                Name = IdentityServerConstants.LocalApi.ScopeName,
                Enabled = true,
                Scopes =
                {
                    new XpoApiResourceScope(unitOfWork)
                    {
                        Scope = "role"
                    }
                }
            };

            await unitOfWork.SaveAsync(idsResource);
            await unitOfWork.CommitChangesAsync();
        }

        if (unitOfWork.Query<XpoIdentityResource>().Count() <= 0)
        {
            AddResource("profile", "Profile", new[] {
                "profile",
                "name",
                "given_name",
                "family_name",
                "middle_name",
                "nickname",
                "preferred_username"
            });

            AddResource("email", "E-mail", new[] {
                "email",
                "email_verified",
            });

            AddResource("openid", "User-Id", new[] {
                "openid",
                "sub",
            });

            AddResource("phone", "Phonenumber", new[] {
                "phone",
                "phone_verified",
            });

            AddResource("role", "Role", new[] {
                "role",
            });

            AddResource("xenial", "Avatar", new[] {
                "xenial",
                "xenial_backcolor",
                "xenial_forecolor",
                "xenial_initials",
            });

            unitOfWork.CommitChanges();
        }

        await CreateRole(AdminRoleName, provider);
        await CreateRole(AuthPolicies.UserManagerRoleName, provider);
        await CreateRole(AuthPolicies.UserManagerReadRoleName, provider);
        await CreateRole(AuthPolicies.UserManagerCreateRoleName, provider);

        if (createAdminUser)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = provider.GetRequiredService<UserManager<XenialIdentityUser>>();
            var user = await userManager.FindByNameAsync(AdminUserName);

            if (user is null)
            {
                var result = await userManager.CreateAsync(new XenialIdentityUser
                {
                    UserName = AdminUserName
                }, AdminPassword);
                user = await userManager.FindByNameAsync(AdminUserName);
                await userManager.AddToRoleAsync(user, AdminRoleName);
                await userManager.SetEmailAsync(user, AdminUserName);
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);
            }
        }

        void AddResource(
            string name,
            string displayName,
            string[] resources,
            bool enabled = true,
            bool showInDiscoveryDocument = true
        )
        {
            var xpoResources = resources.Select(
                type => new XpoIdentityResourceClaim(unitOfWork)
                {
                    Type = type
                }).ToArray();

            var resource = new XpoIdentityResource(unitOfWork)
            {
                Name = name,
                DisplayName = displayName,
                Enabled = true,
                ShowInDiscoveryDocument = true
            };
            resource.UserClaims.AddRange(xpoResources);
            unitOfWork.Save(resource);
        }


        async Task CreateRole(string roleName, IServiceProvider provider)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var role = await roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task UpdateDatabase(
        UnitOfWork unitOfWork,
        IServiceProvider provider,
        IConfiguration configuration
    )
    {
        Log.Information("Update Database");

        var createAdminUser = configuration.GetValue<bool?>("Xenial:SeedAdminUser") ?? false;

        unitOfWork.UpdateSchema(
            false,
            unitOfWork.Dictionary.Classes.OfType<XPClassInfo>().Where(c => c.IsPersistent).ToArray()
        );

        await SeedDatabase(unitOfWork, provider, createAdminUser);
        await provider.GetRequiredService<XpoStringLocalizerService>().Refresh();

        Log.Information("Update Done");
    }
}
