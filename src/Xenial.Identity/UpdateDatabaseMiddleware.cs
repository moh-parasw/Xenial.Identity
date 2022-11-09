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

namespace Xenial.Identity;

public sealed class UpdateDatabaseMiddleware
{
    private static readonly object locker = new();
    private static bool IsDatabaseUpdated { get; set; }

    private readonly RequestDelegate next;
    public UpdateDatabaseMiddleware(RequestDelegate next)
        => this.next = next;

    public async Task InvokeAsync(HttpContext context, [FromServices] DatabaseUpdateHandler handler)
    {
        await handler.UpdateDatabase();
        await next(context);
    }
}

public sealed class DatabaseUpdateHandler
{
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

        if (createAdminUser)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            const string roleName = "Administrator";
            const string userName = "Admin";
            const string password = "!Admin321";
            var role = await roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var userManager = provider.GetRequiredService<UserManager<XenialIdentityUser>>();
            var user = await userManager.FindByNameAsync(userName);

            if (user is null)
            {
                var result = await userManager.CreateAsync(new XenialIdentityUser
                {
                    UserName = userName
                }, password);
                user = await userManager.FindByNameAsync(userName);
                await userManager.AddToRoleAsync(user, roleName);
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
