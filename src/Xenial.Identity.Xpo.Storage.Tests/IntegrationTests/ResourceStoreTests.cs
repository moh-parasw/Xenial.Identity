using System;
using System.Collections.Generic;
using System.Linq;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using IdentityModel;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Stores;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.IntegrationTests
{
    public static class ResourceStoreTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(ResourceStore)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            using var unitOfWork = new UnitOfWork(dataLayer);
            unitOfWork.UpdateSchema();

            (ResourceStore, UnitOfWork) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                return (new ResourceStore(uow, new FakeLogger<ResourceStore>()), uow);
            }

            static IdentityResource CreateIdentityTestResource()
            {
                return new IdentityResource()
                {
                    Name = Guid.NewGuid().ToString(),
                    DisplayName = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    ShowInDiscoveryDocument = true,
                    UserClaims =
                    {
                        JwtClaimTypes.Subject,
                        JwtClaimTypes.Name,
                    }
                };
            }

            static ApiResource CreateApiResourceTestResource()
            {
                return new ApiResource
                {
                    Name = Guid.NewGuid().ToString(),
                    ApiSecrets = new List<Secret> { new Secret("secret".ToSha256()) },
                    Scopes = { Guid.NewGuid().ToString() },
                    UserClaims =
                    {
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                    }
                };
            }

            static ApiScope CreateApiScopeTestResource()
            {
                return new ApiScope
                {
                    Name = Guid.NewGuid().ToString(),
                    UserClaims =
                    {
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                    }
                };
            }

            _ = It("FindApiResourcesByNameAsync when Resource exists should return Resource and Collections", async () =>
            {
                var resource = CreateApiResourceTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();

                    _ = foundResource.Should().NotBeNull();
                    _ = foundResource.Name.Should().Be(resource.Name);

                    _ = foundResource.UserClaims.Should().NotBeNull();
                    _ = foundResource.UserClaims.Should().NotBeEmpty();
                    _ = foundResource.ApiSecrets.Should().NotBeNull();
                    _ = foundResource.ApiSecrets.Should().NotBeEmpty();
                    _ = foundResource.Scopes.Should().NotBeNull();
                    _ = foundResource.Scopes.Should().NotBeEmpty();
                }
            });

            _ = It("FindApiResourcesByNameAsync when Resources exist should only return requested Resources", async () =>
            {
                var resource = CreateApiResourceTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.SaveAsync(CreateApiResourceTestResource().ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();

                    _ = foundResource.Should().NotBeNull();
                    _ = foundResource.Name.Should().Be(resource.Name);

                    _ = foundResource.UserClaims.Should().NotBeNull();
                    _ = foundResource.UserClaims.Should().NotBeEmpty();
                    _ = foundResource.ApiSecrets.Should().NotBeNull();
                    _ = foundResource.ApiSecrets.Should().NotBeEmpty();
                    _ = foundResource.Scopes.Should().NotBeNull();
                    _ = foundResource.Scopes.Should().NotBeEmpty();
                }
            });

            _ = It("FindApiResourcesByScopeNameAsync when Resources exist should return Resources", async () =>
            {
                var testApiResource = CreateApiResourceTestResource();
                var testApiScope = CreateApiScopeTestResource();
                testApiResource.Scopes.Add(testApiScope.Name);

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(testApiResource.ToEntity(uow1));
                    await uow1.SaveAsync(testApiScope.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = await store.FindApiResourcesByScopeNameAsync(new List<string>
                    {
                        testApiScope.Name
                    });

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    _ = resources.SingleOrDefault(x => x.Name == testApiResource.Name)
                        .Should().NotBeNull();
                }
            });

            _ = It("FindApiResourcesByScopeNameAsync when Resources exist should only return Resources requested", async () =>
            {
                var testIdentityResource = CreateIdentityTestResource();
                var testApiResource = CreateApiResourceTestResource();
                var testApiScope = CreateApiScopeTestResource();
                testApiResource.Scopes.Add(testApiScope.Name);

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(testIdentityResource.ToEntity(uow1));
                    await uow1.SaveAsync(testApiResource.ToEntity(uow1));
                    await uow1.SaveAsync(testApiScope.ToEntity(uow1));
                    await uow1.SaveAsync(CreateIdentityTestResource().ToEntity(uow1));
                    await uow1.SaveAsync(CreateApiResourceTestResource().ToEntity(uow1));
                    await uow1.SaveAsync(CreateApiScopeTestResource().ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = await store.FindApiResourcesByScopeNameAsync(new[]
                    {
                        testApiScope.Name
                    });

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    _ = resources.SingleOrDefault(x => x.Name == testApiResource.Name)
                        .Should().NotBeNull();
                }
            });

            _ = It("FindIdentityResourcesByScopeNameAsync when Resource exists should return Resource and Collections", async () =>
            {
                var resource = CreateIdentityTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
                    {
                        resource.Name
                    })).ToList();

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    var foundScope = resources.SingleOrDefault();

                    _ = foundScope.Should().NotBeNull();
                    _ = foundScope.Name.Should().Be(resource.Name);
                    _ = foundScope.UserClaims.Should().NotBeNull();
                    _ = foundScope.UserClaims.Should().NotBeEmpty();
                }
            });

            _ = It("FindIdentityResourcesByScopeNameAsync when Resources exist should only return Requested", async () =>
            {
                var resource = CreateIdentityTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.SaveAsync(CreateIdentityTestResource().ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = (await store.FindIdentityResourcesByScopeNameAsync(new List<string>
                    {
                        resource.Name
                    })).ToList();

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    _ = resources.SingleOrDefault(x => x.Name == resource.Name).Should().NotBeNull();
                }
            });

            _ = It("FindApiScopesByNameAsync when Resource exists should retrun Resource and Collections", async () =>
            {
                var resource = CreateApiScopeTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = (await store.FindApiScopesByNameAsync(new List<string>
                    {
                        resource.Name
                    })).ToList();

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    var foundScope = resources.SingleOrDefault();

                    _ = foundScope.Should().NotBeNull();
                    _ = foundScope.Name.Should().Be(resource.Name);
                    _ = foundScope.UserClaims.Should().NotBeNull();
                    _ = foundScope.UserClaims.Should().NotBeEmpty();
                }
            });

            _ = It("FindApiScopesByNameAsync when Resources exist should only return Requested", async () =>
            {
                var resource = CreateApiScopeTestResource();

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(resource.ToEntity(uow1));
                    await uow1.SaveAsync(CreateApiScopeTestResource().ToEntity(uow1));
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = (await store.FindApiScopesByNameAsync(new List<string>
                    {
                        resource.Name
                    })).ToList();

                    _ = resources.Should().NotBeNull();
                    _ = resources.Should().NotBeEmpty();
                    _ = resources.Single().Name.Should().Be(resource.Name);
                }
            });

            _ = It("GetAllResources when all Resources are requested should return all Resources including hidden ones", async () =>
            {
                var visibleIdentityResource = CreateIdentityTestResource();
                var visibleApiResource = CreateApiResourceTestResource();
                var visibleApiScope = CreateApiScopeTestResource();
                var hiddenIdentityResource = new IdentityResource { Name = Guid.NewGuid().ToString(), ShowInDiscoveryDocument = false };
                var hiddenApiResource = new ApiResource
                {
                    Name = Guid.NewGuid().ToString(),
                    Scopes = { Guid.NewGuid().ToString() },
                    ShowInDiscoveryDocument = false
                };
                var hiddenApiScope = new ApiScope
                {
                    Name = Guid.NewGuid().ToString(),
                    ShowInDiscoveryDocument = false
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    await uow1.SaveAsync(visibleIdentityResource.ToEntity(uow1));
                    await uow1.SaveAsync(visibleApiResource.ToEntity(uow1));
                    await uow1.SaveAsync(visibleApiScope.ToEntity(uow1));

                    await uow1.SaveAsync(hiddenIdentityResource.ToEntity(uow1));
                    await uow1.SaveAsync(hiddenApiResource.ToEntity(uow1));
                    await uow1.SaveAsync(hiddenApiScope.ToEntity(uow1));

                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var resources = await store.GetAllResourcesAsync();

                    _ = resources.Should().NotBeNull();
                    _ = resources.IdentityResources.Should().NotBeEmpty();
                    _ = resources.ApiResources.Should().NotBeEmpty();
                    _ = resources.ApiScopes.Should().NotBeEmpty();

                    _ = resources.IdentityResources.Should().Contain(x => x.Name == visibleIdentityResource.Name);
                    _ = resources.IdentityResources.Should().Contain(x => x.Name == hiddenIdentityResource.Name);

                    _ = resources.ApiResources.Should().Contain(x => x.Name == visibleApiResource.Name);
                    _ = resources.ApiResources.Should().Contain(x => x.Name == hiddenApiResource.Name);

                    _ = resources.ApiScopes.Should().Contain(x => x.Name == visibleApiScope.Name);
                    _ = resources.ApiScopes.Should().Contain(x => x.Name == hiddenApiScope.Name);
                }
            });
        });
    }
}
