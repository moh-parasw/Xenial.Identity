using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Xenial.Identity.Infrastructure.Localization;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Models;

using Xenial.Identity.Xpo.Storage.Models;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using DevExpress.Xpo.Metadata;

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

    public DatabaseUpdateHandler(IServiceProvider provider)
        => this.provider = provider;

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
                await UpdateDatabase(uow, scope.ServiceProvider);
            }
        }
        finally
        {
            IsDatabaseUpToDate = true;
        }
    }

    private static void SeedDatabase(UnitOfWork unitOfWork)
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
        IServiceProvider provider)
    {
        Log.Information("Update Database");

        unitOfWork.UpdateSchema(
            false,
            unitOfWork.Dictionary.Classes.OfType<XPClassInfo>().Where(c => c.IsPersistent).ToArray()
        );
        SeedDatabase(unitOfWork);
        await provider.GetRequiredService<XpoStringLocalizerService>().Refresh();

        Log.Information("Update Done");
    }
}
