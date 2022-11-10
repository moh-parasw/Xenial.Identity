using DevExpress.Xpo.DB;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage;

namespace Xenial.Identity.Infrastructure
{
    public static class XpoExtentions
    {
        public static IServiceCollection AddXpo(
            this IServiceCollection services,
            IConfiguration configuration,
            AutoCreateOption autoCreateOption = AutoCreateOption.DatabaseAndSchema
        )
            => services.AddXpoDefaultDataLayer(ServiceLifetime.Singleton, dl => dl
                .UseConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .UseThreadSafeDataLayer(autoCreateOption == AutoCreateOption.DatabaseAndSchema)
                .UseConnectionPool(autoCreateOption == AutoCreateOption.DatabaseAndSchema)
                .UseAutoCreationOption(autoCreateOption)
                .UseEntityTypes(
                    IdentityXpoTypes.PersistentTypes
                        .Concat(IdentityModelTypeList.ModelTypes)
                        .Concat(XenialIdentityModelTypeList.ModelTypes)
                    .ToArray()
                )
            );
    }
}
