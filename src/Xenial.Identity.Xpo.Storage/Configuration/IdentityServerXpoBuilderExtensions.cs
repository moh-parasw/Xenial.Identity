using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Xenial.Identity.Xpo.Storage.Options;
using Xenial.Identity.Xpo.Storage.TokenCleanup;

namespace Xenial.Identity.Xpo.Storage.Configuration
{
    /// <summary>
    /// Extension methods to add XPO database support to IdentityServer.
    /// </summary>
    public static class IdentityServerXpoBuilderExtensions
    {
        /// <summary>
        /// Adds operational DbContext to the DI system.
        /// </summary>
        /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddXpoIdentityStorage(this IServiceCollection services,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            var storeOptions = new OperationalStoreOptions();
            services.AddSingleton(storeOptions);
            storeOptionsAction?.Invoke(storeOptions);

            services.AddTransient<TokenCleanupService>();

            return services;
        }

        /// <summary>
        /// Adds an implementation of the IOperationalStoreNotification to the DI system.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddXpoStoreNotification<T>(this IServiceCollection services)
           where T : class, IOperationalStoreNotification
            => services.AddTransient<IOperationalStoreNotification, T>();
    }
}
