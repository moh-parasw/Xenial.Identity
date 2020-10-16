using System;
using System.Collections.Generic;
using System.Linq;

using DevExpress.Xpo;

using FluentAssertions;

using IdentityModel;

using IdentityServer4.Models;

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
                => new IdentityResource()
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

            static ApiResource CreateApiResourceTestResource()
                => new ApiResource
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

            static ApiScope CreateApiScopeTestResource()
                => new ApiScope
                {
                    Name = Guid.NewGuid().ToString(),
                    UserClaims =
                    {
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                    }
                };

            It("FindApiResourcesByNameAsync when Resource exists should return Resource and Collections", async () =>
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

                    foundResource.Should().NotBeNull();
                    foundResource.Name.Should().Be(resource.Name);

                    foundResource.UserClaims.Should().NotBeNull();
                    foundResource.UserClaims.Should().NotBeEmpty();
                    foundResource.ApiSecrets.Should().NotBeNull();
                    foundResource.ApiSecrets.Should().NotBeEmpty();
                    foundResource.Scopes.Should().NotBeNull();
                    foundResource.Scopes.Should().NotBeEmpty();
                }
            });

            It("FindApiResourcesByNameAsync when Resources exist should only return requested Resources", async () =>
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

                    foundResource.Should().NotBeNull();
                    foundResource.Name.Should().Be(resource.Name);

                    foundResource.UserClaims.Should().NotBeNull();
                    foundResource.UserClaims.Should().NotBeEmpty();
                    foundResource.ApiSecrets.Should().NotBeNull();
                    foundResource.ApiSecrets.Should().NotBeEmpty();
                    foundResource.Scopes.Should().NotBeNull();
                    foundResource.Scopes.Should().NotBeEmpty();
                }
            });

            It("FindApiResourcesByScopeNameAsync when Resources exist should return Resources", async () =>
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

                    resources.Should().NotBeNull();
                    resources.Should().NotBeEmpty();
                    resources.SingleOrDefault(x => x.Name == testApiResource.Name)
                        .Should().NotBeNull();
                }
            });

            It("FindApiResourcesByScopeNameAsync when Resources exist should only return Resources requested", async () =>
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

                    resources.Should().NotBeNull();
                    resources.Should().NotBeEmpty();
                    resources.SingleOrDefault(x => x.Name == testApiResource.Name)
                        .Should().NotBeNull();
                }
            });
        });
    }
}
