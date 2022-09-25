using System;
using System.Threading.Tasks;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Stores;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.IntegrationTests
{
    public static class ClientStoreTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(ClientStore)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            using var unitOfWork = new UnitOfWork(dataLayer);
            unitOfWork.UpdateSchema();

            (ClientStore, UnitOfWork) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                return (new ClientStore(uow, new FakeLogger<ClientStore>()), uow);
            }

            _ = It("FindClientByIdAsync when client does not exist returns null", async () =>
            {
                var (store, uow) = CreateStore();
                using (uow)
                {
                    var client = await store.FindClientByIdAsync(Guid.NewGuid().ToString());
                    _ = client.Should().BeNull();
                }
            });

            _ = It("FindClientByIdAsync when client exists returns existing client", async () =>
            {
                var testClient = new Client
                {
                    ClientId = "test_client",
                    ClientName = "Test Client"
                };

                using (var unitOfWork = new UnitOfWork(dataLayer))
                {
                    _ = testClient.ToEntity(unitOfWork);
                    await unitOfWork.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var client = await store.FindClientByIdAsync(testClient.ClientId);

                    _ = client.Should().NotBeNull();
                }
            });

            _ = It("FindClientByIdAsync when client exists with collections should return client with collections", async () =>
            {
                var testClient = new Client
                {
                    ClientId = "properties_test_client",
                    ClientName = "Properties Test Client",
                    AllowedCorsOrigins = { "https://localhost" },
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowedScopes = { "openid", "profile", "api1" },
                    Claims = { new ClientClaim("test", "value") },
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    IdentityProviderRestrictions = { "AD" },
                    PostLogoutRedirectUris = { "https://locahost/signout-callback" },
                    Properties = { { "foo1", "bar1" }, { "foo2", "bar2" }, },
                    RedirectUris = { "https://locahost/signin" }
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = testClient.ToEntity(uow1);
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var client = await store.FindClientByIdAsync(testClient.ClientId);
                    _ = client.Should().BeEquivalentTo(testClient);
                }
            });

            _ = It("FindClientByIdAsync when clients exist with many collections should return in under five seconds", async () =>
            {
                var testClient = new Client
                {
                    ClientId = "test_client_with_uris",
                    ClientName = "Test client with URIs",
                    AllowedScopes = { "openid", "profile", "api1" },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials
                };

                for (var i = 0; i < 50; i++)
                {
                    testClient.RedirectUris.Add($"https://localhost/{i}");
                    testClient.PostLogoutRedirectUris.Add($"https://localhost/{i}");
                    testClient.AllowedCorsOrigins.Add($"https://localhost:{i}");
                }

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = testClient.ToEntity(uow1);

                    for (var i = 0; i < 50; i++)
                    {
                        _ = new Client()
                        {
                            ClientId = testClient.ClientId + i,
                            ClientName = testClient.ClientName,
                            AllowedScopes = testClient.AllowedScopes,
                            AllowedGrantTypes = testClient.AllowedGrantTypes,
                            RedirectUris = testClient.RedirectUris,
                            PostLogoutRedirectUris = testClient.PostLogoutRedirectUris,
                            AllowedCorsOrigins = testClient.AllowedCorsOrigins,
                        }.ToEntity(uow1);
                    }

                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    const int timeout = 5000;
                    var task = Task.Run(() => store.FindClientByIdAsync(testClient.ClientId));

                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                        var client = await task;
                        _ = client.Should().BeEquivalentTo(testClient);
                    }
                    else
                    {
                        throw new TestTimeoutException(timeout);
                    }
                }
            });
        });
    }
}
