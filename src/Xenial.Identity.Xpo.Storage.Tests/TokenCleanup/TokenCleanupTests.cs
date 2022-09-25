using System;
using System.Collections.Generic;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Test;

using FluentAssertions;

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
                _ = services.AddIdentityServer()
                    .AddTestUsers(new List<TestUser>())
                    .AddInMemoryClients(new List<Client>())
                    .AddInMemoryIdentityResources(new List<IdentityResource>())
                    .AddInMemoryApiResources(new List<ApiResource>());

                _ = services.AddTransient(_ =>
                {
                    var uow = new UnitOfWork(dataLayer);
                    uow.UpdateSchema();
                    return uow;
                });

                _ = services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
                _ = services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();

                _ = services.AddTransient<TokenCleanupService>();
                _ = services.AddSingleton(storeOptions);

                return services.BuildServiceProvider().GetRequiredService<TokenCleanupService>();
            }

            _ = It("RemoveExpiredGrantsAsync when expired grants exist should remove expired grants", async () =>
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

                using var uow2 = new UnitOfWork(dataLayer);
                _ = (await uow2.Query<XpoPersistedGrant>()
                    .FirstOrDefaultAsync(x => x.Key == key)
                ).Should().BeNull();
            });

            _ = It("RemoveExpiredGrantsAsync when valid Grants exist expect valid Grants in Db", async () =>
            {
                var key = Guid.NewGuid().ToString();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var validGrant = new XpoPersistedGrant(uow1)
                    {
                        Key = key,
                        ClientId = "app1",
                        Type = "reference",
                        SubjectId = "123",
                        Expiration = DateTime.UtcNow.AddDays(3),
                        Data = "{!}"
                    };

                    await uow1.SaveAsync(validGrant);
                    await uow1.CommitChangesAsync();
                }

                await CreateSut().RemoveExpiredGrantsAsync();

                using var uow2 = new UnitOfWork(dataLayer);
                _ = (await uow2.Query<XpoPersistedGrant>()
                    .FirstOrDefaultAsync(x => x.Key == key)
                ).Should().NotBeNull();
            });

            _ = It("RemoveExpiredGrantsAsync when expired DeviceGrants exist expect expired DeviceGrants to be removed", async () =>
            {
                var deviceCode = Guid.NewGuid().ToString();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var expiredGrant = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = deviceCode,
                        UserCode = Guid.NewGuid().ToString(),
                        ClientId = "app1",
                        SubjectId = "123",
                        CreationTime = DateTime.UtcNow.AddDays(-4),
                        Expiration = DateTime.UtcNow.AddDays(-3),
                        Data = "{!}"
                    };
                    await uow1.SaveAsync(expiredGrant);
                    await uow1.CommitChangesAsync();
                }

                await CreateSut().RemoveExpiredGrantsAsync();

                using var uow2 = new UnitOfWork(dataLayer);
                _ = (await uow2.Query<XpoDeviceFlowCodes>()
                    .FirstOrDefaultAsync(x => x.DeviceCode == deviceCode)
                ).Should().BeNull();
            });

            _ = It("RemoveExpiredGrantsAsync when valid DeviceGrants exist expect valid DeviceGrants to be in Db", async () =>
            {
                var deviceCode = Guid.NewGuid().ToString();
                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var validGrant = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = deviceCode,
                        UserCode = "2468",
                        ClientId = "app1",
                        SubjectId = "123",
                        CreationTime = DateTime.UtcNow.AddDays(-4),
                        Expiration = DateTime.UtcNow.AddDays(3),
                        Data = "{!}"
                    };
                    await uow1.SaveAsync(validGrant);
                    await uow1.CommitChangesAsync();
                }

                await CreateSut().RemoveExpiredGrantsAsync();

                using var uow2 = new UnitOfWork(dataLayer);
                _ = (await uow2.Query<XpoDeviceFlowCodes>()
                    .FirstOrDefaultAsync(x => x.DeviceCode == deviceCode)
                ).Should().NotBeNull();
            });
        });
    }
}
