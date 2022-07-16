using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Xpo;

using FluentAssertions;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

using Xenial.Identity.Xpo.Storage.Mappers;
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

            It("StoreAsync when PersistedGrant is stored should succeed", async () =>
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

            It("GetAsync with Key and existing PersistedGrant expect PersistedGrant to be returned", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var foundPersistedGrant = await store.GetAsync(persistedGrant.Key);
                    foundPersistedGrant.Should().NotBeNull();
                }
            });

            It("GetAllAsync with Sub and Type and existing PersistedGrant expect PersistedGrant to be returned", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var foundPersistedGrants = (await store.GetAllAsync(new PersistedGrantFilter { SubjectId = persistedGrant.SubjectId })).ToList();
                    foundPersistedGrants.Should().NotBeNull();
                    foundPersistedGrants.Should().NotBeEmpty();
                }
            });

            It("GetAllAsync should filter", async () =>
            {
                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity(uow1));
                    await uow1.SaveAsync(CreateTestObject().ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1"
                    })).ToList().Count.Should().Be(9);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub2"
                    })).ToList().Count.Should().Be(0);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1"
                    })).ToList().Count.Should().Be(4);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c2"
                    })).ToList().Count.Should().Be(4);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c3"
                    })).ToList().Count.Should().Be(1);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c4"
                    })).ToList().Count.Should().Be(0);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1"
                    })).ToList().Count.Should().Be(2);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c3",
                        SessionId = "s1"
                    })).ToList().Count.Should().Be(0);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1",
                        Type = "t1"
                    })).ToList().Count.Should().Be(1);
                    (await store.GetAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1",
                        Type = "t3"
                    })).ToList().Count.Should().Be(0);
                }
            });

            It("RemoveAsync when Key of existing received should be deleted", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.RemoveAsync(persistedGrant.Key);
                }

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow2.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);

                    foundGrant.Should().BeNull();
                }
            });

            It("RemoveAllAsync when SubId and ClientId of existing received should be deleted", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = persistedGrant.SubjectId,
                        ClientId = persistedGrant.ClientId
                    });
                }

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow2.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);

                    foundGrant.Should().BeNull();
                }
            });

            It("RemoveAllAsync when SubId, ClientId and Type of existing received should be deleted", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = persistedGrant.SubjectId,
                        ClientId = persistedGrant.ClientId,
                        Type = persistedGrant.Type
                    });
                }

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow2.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);

                    foundGrant.Should().BeNull();
                }
            });

            It("RemoveAllAsync should filter", async () =>
            {
                async Task PopulateDb()
                {
                    using (var uow = new UnitOfWork(dataLayer))
                    {
                        await uow.DeleteAsync(await uow.Query<XpoPersistedGrant>().ToListAsync());
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity(uow));
                        await uow.SaveAsync(CreateTestObject().ToEntity(uow));
                        await uow.CommitChangesAsync();
                    }
                }

                await PopulateDb();
                var (store1, uow1) = CreateStore();
                using (uow1)
                {
                    await store1.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1"
                    });

                    (await uow1.Query<XpoPersistedGrant>().CountAsync()).Should().Be(1);
                }

                await PopulateDb();
                var (store2, uow2) = CreateStore();
                using (uow2)
                {
                    await store2.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub2"
                    });
                    (await uow2.Query<XpoPersistedGrant>().CountAsync()).Should().Be(10);
                }

                await PopulateDb();
                var (store3, uow3) = CreateStore();
                using (uow3)
                {
                    await store3.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1"
                    });
                    (await uow3.Query<XpoPersistedGrant>().CountAsync()).Should().Be(6);
                }

                await PopulateDb();
                var (store4, uow4) = CreateStore();
                using (uow4)
                {
                    await store4.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c2"
                    });
                    (await uow4.Query<XpoPersistedGrant>().CountAsync()).Should().Be(6);
                }

                await PopulateDb();
                var (store5, uow5) = CreateStore();
                using (uow5)
                {
                    await store5.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c3"
                    });
                    (await uow5.Query<XpoPersistedGrant>().CountAsync()).Should().Be(9);
                }


                await PopulateDb();
                var (store6, uow6) = CreateStore();
                using (uow6)
                {
                    await store6.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c4"
                    });
                    (await uow6.Query<XpoPersistedGrant>().CountAsync()).Should().Be(10);
                }

                await PopulateDb();
                var (store7, uow7) = CreateStore();
                using (uow7)
                {
                    await store7.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1"
                    });
                    (await uow7.Query<XpoPersistedGrant>().CountAsync()).Should().Be(8);
                }

                await PopulateDb();
                var (store8, uow8) = CreateStore();
                using (uow8)
                {
                    await store8.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c3",
                        SessionId = "s1"
                    });
                    (await uow8.Query<XpoPersistedGrant>().CountAsync()).Should().Be(10);
                }

                await PopulateDb();
                var (store9, uow9) = CreateStore();
                using (uow9)
                {
                    await store9.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1",
                        Type = "t1"
                    });
                    (await uow9.Query<XpoPersistedGrant>().CountAsync()).Should().Be(9);
                }

                await PopulateDb();
                var (store10, uow10) = CreateStore();
                using (uow10)
                {
                    await store10.RemoveAllAsync(new PersistedGrantFilter
                    {
                        SubjectId = "sub1",
                        ClientId = "c1",
                        SessionId = "s1",
                        Type = "t3"
                    });
                    (await uow10.Query<XpoPersistedGrant>().CountAsync()).Should().Be(10);
                }
            });


            It("Store should create new record if key does not exist", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow1.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);

                    foundGrant.Should().BeNull();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.StoreAsync(persistedGrant);
                }

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow2.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);

                    foundGrant.Should().NotBeNull();
                }
            });

            It("Store should update record if key already exists", async () =>
            {
                var persistedGrant = CreateTestObject();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(persistedGrant.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var newDate = persistedGrant.Expiration.Value.AddHours(1);
                var (store, uow) = CreateStore();
                using (uow)
                {
                    persistedGrant.Expiration = newDate;
                    await store.StoreAsync(persistedGrant);
                }

                using (var uow2 = new UnitOfWork(dataLayer))
                {
                    var foundGrant = await uow2.Query<XpoPersistedGrant>().FirstOrDefaultAsync(x => x.Key == persistedGrant.Key);
                    foundGrant.Should().NotBeNull();
                    foundGrant.Expiration.Should().Be(newDate);
                }
            });
        });
    }
}
