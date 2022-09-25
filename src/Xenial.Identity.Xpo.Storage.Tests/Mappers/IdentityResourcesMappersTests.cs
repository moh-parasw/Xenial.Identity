using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class IdentityResourceMappersTests
    {
        public static void Tests() => Describe(nameof(IdentityResourceMappers), () =>
        {
            _ = It("IdentityResource Automapper Configuration is valid", () => IdentityResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<IdentityResourceMapperProfile>());

            _ = It("Can map IdentityResources", () =>
            {
                using var session = new Session();
                var model = new IdentityResource();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Should().NotBeNull();
                _ = mappedEntity.Should().NotBeNull();
            });
        });
    }
}
