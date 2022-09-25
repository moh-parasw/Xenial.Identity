using System.Linq;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class ScopesMappersTests
    {
        public static void Tests() => Describe(nameof(ScopeMappers), () =>
        {
            _ = It("Scope Automapper Configuration is valid", () => ScopeMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<ScopeMapperProfile>());

            _ = It("Can map scope", () =>
            {
                using var session = new Session();
                var model = new ApiScope();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Should().NotBeNull();
                _ = mappedEntity.Should().NotBeNull();
            });

            _ = It("Properties map", () =>
            {
                using var session = new Session();

                var model = new ApiScope()
                {
                    Description = "description",
                    DisplayName = "displayname",
                    Name = "foo",
                    UserClaims = { "c1", "c2" },
                    Properties = {
                        { "x", "xx" },
                        { "y", "yy" },
                    },
                    Enabled = false
                };


                var mappedEntity = model.ToEntity(session);
                _ = mappedEntity.Description.Should().Be("description");
                _ = mappedEntity.DisplayName.Should().Be("displayname");
                _ = mappedEntity.Name.Should().Be("foo");

                _ = mappedEntity.UserClaims.Count.Should().Be(2);
                _ = mappedEntity.UserClaims.Select(x => x.Type).Should().BeEquivalentTo(new[] { "c1", "c2" });
                _ = mappedEntity.Properties.Count.Should().Be(2);
                _ = mappedEntity.Properties.Should().Contain(x => x.Key == "x" && x.Value == "xx");
                _ = mappedEntity.Properties.Should().Contain(x => x.Key == "y" && x.Value == "yy");


                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Description.Should().Be("description");
                _ = mappedModel.DisplayName.Should().Be("displayname");
                _ = mappedModel.Enabled.Should().BeFalse();
                _ = mappedModel.Name.Should().Be("foo");
                _ = mappedModel.UserClaims.Count.Should().Be(2);
                _ = mappedModel.UserClaims.Should().BeEquivalentTo(new[] { "c1", "c2" });
                _ = mappedModel.Properties.Count.Should().Be(2);
                _ = mappedModel.Properties["x"].Should().Be("xx");
                _ = mappedModel.Properties["y"].Should().Be("yy");
            });
        });
    }
}
