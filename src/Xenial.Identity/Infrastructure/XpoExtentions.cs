using System.Linq;

using DevExpress.Xpo.DB;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            AutoCreateOption autoCreateOption = AutoCreateOption.None
        )
            => services.AddXpoDefaultDataLayer(ServiceLifetime.Singleton, dl => dl
                .UseConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .UseThreadSafeDataLayer(autoCreateOption == AutoCreateOption.None)
                .UseConnectionPool(autoCreateOption == AutoCreateOption.None)
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
