using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.Xpo;
using IdentityModel;
using IdentityServer4.Models;
using Xenial.Identity.Xpo.Storage.Stores;
using Xenial.Identity.Xpo.Storage.Mappers;
using static Xenial.Tasty;
using FluentAssertions;
using System.Linq;

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

            // static IdentityResource CreateIdentityTestResource()
            //     => new IdentityResource()
            //     {
            //         Name = Guid.NewGuid().ToString(),
            //         DisplayName = Guid.NewGuid().ToString(),
            //         Description = Guid.NewGuid().ToString(),
            //         ShowInDiscoveryDocument = true,
            //         UserClaims =
            //         {
            //             JwtClaimTypes.Subject,
            //             JwtClaimTypes.Name,
            //         }
            //     };

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

            // static ApiScope CreateApiScopeTestResource()
            //     => new ApiScope
            //     {
            //         Name = Guid.NewGuid().ToString(),
            //         UserClaims =
            //         {
            //                 Guid.NewGuid().ToString(),
            //                 Guid.NewGuid().ToString(),
            //         }
            //     };

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
        });
    }
}
