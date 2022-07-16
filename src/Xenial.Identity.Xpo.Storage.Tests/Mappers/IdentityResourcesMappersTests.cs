using DevExpress.Xpo;

using FluentAssertions;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class IdentityResourceMappersTests
    {
        public static void Tests() => Describe(nameof(IdentityResourceMappers), () =>
        {
            It("IdentityResource Automapper Configuration is valid", () => IdentityResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<IdentityResourceMapperProfile>());

            It("Can map IdentityResources", () =>
            {
                using var session = new Session();
                var model = new IdentityResource();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                mappedModel.Should().NotBeNull();
                mappedEntity.Should().NotBeNull();
            });
        });
    }
}
