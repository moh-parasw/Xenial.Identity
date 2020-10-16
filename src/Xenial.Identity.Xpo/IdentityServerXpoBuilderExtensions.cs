using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Xenial.Identity.Xpo.Storage.TokenCleanup;
using Xenial.Identity.Xpo.Storage.Configuration;
using Microsoft.Extensions.Hosting;
using IdentityServer4.Stores;
using Xenial.Identity.Xpo.Storage.Stores;
using Xenial.Identity.Xpo.Storage.Options;
using Xenial.Identity.Xpo.Services;

namespace Microsoft.Extensions.DependencyInjection
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
            builder.Services.AddXpoIdentityStorage(storeOptionsAction);

            builder.AddClientStore<ClientStore>();
            builder.AddResourceStore<ResourceStore>();
            builder.AddCorsPolicyService<CorsPolicyService>();
            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            builder.Services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
            builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

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
            builder.Services.AddXpoStoreNotification<T>();
            return builder;
        }
    }
}
