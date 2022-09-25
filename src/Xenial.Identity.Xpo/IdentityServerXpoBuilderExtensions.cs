using System;

using Duende.IdentityServer.Stores;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xenial.Identity.Xpo;
using Xenial.Identity.Xpo.Services;
using Xenial.Identity.Xpo.Storage.Configuration;
using Xenial.Identity.Xpo.Storage.Options;
using Xenial.Identity.Xpo.Storage.Stores;
using Xenial.Identity.Xpo.Storage.TokenCleanup;

namespace Xenial.Identity.Xpo
{
    public static class IdentityServerXpoBuilderExtensions
    {
        /// <summary>
        /// Configures Xpo implementation of IPersistedGrantStore with IdentityServer.
        /// </summary>
        /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddXpoIdentityStore(
            this IIdentityServerBuilder builder,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            _ = builder.Services.AddXpoIdentityStorage(storeOptionsAction);

            _ = builder.AddClientStore<ClientStore>();
            _ = builder.AddResourceStore<ResourceStore>();
            _ = builder.AddCorsPolicyService<CorsPolicyService>();
            _ = builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            _ = builder.Services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
            _ = builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

            return builder;
        }

        /// <summary>
        /// Adds an implementation of the IOperationalStoreNotification to IdentityServer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddXpoStoreNotification<T>(
           this IIdentityServerBuilder builder)
           where T : class, IOperationalStoreNotification
        {
            _ = builder.Services.AddXpoStoreNotification<T>();
            return builder;
        }
    }
}
