using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class PersistedGrantMappersTests
    {
        public static void Tests() => Describe(nameof(PersistedGrantMappers), () =>
        {
            _ = It("PersistedGrant Automapper Configuration is valid", () => PersistedGrantMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<PersistedGrantMapperProfile>());

            _ = It("Can map", () =>
            {
                using var session = new Session();
                var model = new PersistedGrant()
                {
                    ConsumedTime = new System.DateTime(2020, 02, 03, 4, 5, 6)
                };

                var mappedEntity = model.ToEntity(session);
                _ = mappedEntity.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));

                var mappedModel = mappedEntity.ToModel();
                _ = mappedModel.ConsumedTime.Value.Should().Be(new System.DateTime(2020, 02, 03, 4, 5, 6));

                _ = mappedModel.Should().NotBeNull();
                _ = mappedEntity.Should().NotBeNull();

            });
        });
    }
}
