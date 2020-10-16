using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

using FluentAssertions;

using IdentityServer4.Models;

using Xenial.Identity.Xpo.Storage.Models;
using Xenial.Identity.Xpo.Storage.Stores;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.IntegrationTests
{
    public static class PersistedGrantStoreTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(PersistedGrantStore)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            using var unitOfWork = new UnitOfWork(dataLayer);
            unitOfWork.UpdateSchema();

            (PersistedGrantStore, UnitOfWork) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                return (new PersistedGrantStore(uow, new FakeLogger<PersistedGrantStore>()), uow);
            }

            static PersistedGrant CreateTestObject(string sub = null, string clientId = null, string sid = null, string type = null)
                => new PersistedGrant
                {
                    Key = Guid.NewGuid().ToString(),
                    Type = type ?? "authorization_code",
                    ClientId = clientId ?? Guid.NewGuid().ToString(),
                    SubjectId = sub ?? Guid.NewGuid().ToString(),
                    SessionId = sid ?? Guid.NewGuid().ToString(),
                    CreationTime = new DateTime(2016, 08, 01),
                    Expiration = new DateTime(2016, 08, 31),
                    Data = Guid.NewGuid().ToString()
                };

            It("StoreAsync_WhenPersistedGrantStored_ExpectSuccess", async () =>
            {
                var persistedGrant = CreateTestObject();

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.StoreAsync(persistedGrant);
                }

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow1.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);
                    foundGrant.Should().NotBeNull();
                }
            });
        });
    }
}
