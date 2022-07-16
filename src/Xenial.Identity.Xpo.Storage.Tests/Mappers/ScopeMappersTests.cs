using System.Linq;

using DevExpress.Xpo;

using FluentAssertions;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Mappers;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class ScopesMappersTests
    {
        public static void Tests() => Describe(nameof(ScopeMappers), () =>
        {
            It("Scope Automapper Configuration is valid", () => ScopeMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<ScopeMapperProfile>());

            It("Can map scope", () =>
            {
                using var session = new Session();
                var model = new ApiScope();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                mappedModel.Should().NotBeNull();
                mappedEntity.Should().NotBeNull();
            });

            It("Properties map", () =>
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
                mappedEntity.Description.Should().Be("description");
                mappedEntity.DisplayName.Should().Be("displayname");
                mappedEntity.Name.Should().Be("foo");

                mappedEntity.UserClaims.Count.Should().Be(2);
                mappedEntity.UserClaims.Select(x => x.Type).Should().BeEquivalentTo(new[] { "c1", "c2" });
                mappedEntity.Properties.Count.Should().Be(2);
                mappedEntity.Properties.Should().Contain(x => x.Key == "x" && x.Value == "xx");
                mappedEntity.Properties.Should().Contain(x => x.Key == "y" && x.Value == "yy");


                var mappedModel = mappedEntity.ToModel();

                mappedModel.Description.Should().Be("description");
                mappedModel.DisplayName.Should().Be("displayname");
                mappedModel.Enabled.Should().BeFalse();
                mappedModel.Name.Should().Be("foo");
                mappedModel.UserClaims.Count.Should().Be(2);
                mappedModel.UserClaims.Should().BeEquivalentTo(new[] { "c1", "c2" });
                mappedModel.Properties.Count.Should().Be(2);
                mappedModel.Properties["x"].Should().Be("xx");
                mappedModel.Properties["y"].Should().Be("yy");
            });
        });
    }
}
