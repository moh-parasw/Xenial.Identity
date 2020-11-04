using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage;

namespace Xenial.Identity.Infrastructure
{
    public static class XpoExtentions
    {
        public static IServiceCollection AddXpo(this IServiceCollection services, IConfiguration configuration, DevExpress.Xpo.DB.AutoCreateOption autoCreateOption = DevExpress.Xpo.DB.AutoCreateOption.None)
            => services.AddXpoDefaultDataLayer(ServiceLifetime.Singleton, dl => dl
                .UseConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .UseThreadSafeDataLayer(true)
                .UseConnectionPool(autoCreateOption == DevExpress.Xpo.DB.AutoCreateOption.None)
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
