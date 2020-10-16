using System;
using System.Collections.Generic;

using DevExpress.Xpo;

using FluentAssertions;

using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Test;

using Microsoft.Extensions.DependencyInjection;

using Xenial.Identity.Xpo.Storage.Models;
using Xenial.Identity.Xpo.Storage.Options;
using Xenial.Identity.Xpo.Storage.Stores;
using Xenial.Identity.Xpo.Storage.TokenCleanup;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.TokenCleanup
{
    public static class TokenCleanupTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(TokenCleanupService)} using {name}", () =>
        {
            var storeOptions = new OperationalStoreOptions();
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            TokenCleanupService CreateSut()
            {
                IServiceCollection services = new ServiceCollection();
                services.AddIdentityServer()
                    .AddTestUsers(new List<TestUser>())
                    .AddInMemoryClients(new List<Client>())
                    .AddInMemoryIdentityResources(new List<IdentityResource>())
                    .AddInMemoryApiResources(new List<ApiResource>());

                services.AddTransient(_ =>
                {
                    var uow = new UnitOfWork(dataLayer);
                    uow.UpdateSchema();
                    return uow;
                });

                services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
                services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();

                services.AddTransient<TokenCleanupService>();
                services.AddSingleton(storeOptions);

                return services.BuildServiceProvider().GetRequiredService<TokenCleanupService>();
            }

            It("RemoveExpiredGrantsAsync when expired grants exist should remove expired grants", async () =>
            {
                var key = Guid.NewGuid().ToString();
                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var expiredGrant = new XpoPersistedGrant(uow1)
                    {
                        Key = key,
                        ClientId = "app1",
                        Type = "reference",
                        SubjectId = "123",
                        Expiration = DateTime.UtcNow.AddDays(-3),
                        Data = "{!}"
                    };

                    await uow1.SaveAsync(expiredGrant);
                    await uow1.CommitChangesAsync();
                }

                await CreateSut().RemoveExpiredGrantsAsync();

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    (await uow2.Query<XpoPersistedGrant>()
                        .FirstOrDefaultAsync(x => x.Key == key)
                    ).Should().BeNull();
                }
            });
        });
    }
}
