using System.Linq;

using DevExpress.Xpo;

using FluentAssertions;

using Duende.IdentityServer.Models;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class ApiResourceMappersTests
    {
        public static void Tests() => Describe(nameof(ApiResourceMappers), () =>
        {
            It("Automapper Configuration is valid", () => ApiResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<ApiResourceMapperProfile>());

            It("Can map", () =>
            {
                using var session = new Session();
                var model = new ApiResource();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                mappedModel.Should().NotBeNull();
                mappedEntity.Should().NotBeNull();
            });

            It("Properties map", () =>
            {
                using var session = new Session();
                var model = new ApiResource()
                {
                    Description = "description",
                    DisplayName = "displayname",
                    Name = "foo",
                    Scopes = { "foo1", "foo2" },
                    Enabled = false
                };


                var mappedEntity = model.ToEntity(session);

                mappedEntity.Scopes.Count.Should().Be(2);
                var foo1 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo1");
                foo1.Should().NotBeNull();
                var foo2 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo2");
                foo2.Should().NotBeNull();


                var mappedModel = mappedEntity.ToModel();

                mappedModel.Description.Should().Be("description");
                mappedModel.DisplayName.Should().Be("displayname");
                mappedModel.Enabled.Should().BeFalse();
                mappedModel.Name.Should().Be("foo");
            });

            It("Missing values should use defaults", () =>
            {
                using var session = new Session();
                var entity = new XpoApiResource(session)
                {
                    Secrets =
                    {
                        new XpoApiResourceSecret(session)
                    }
                };

                var def = new ApiResource
                {
                    ApiSecrets = { new Secret("foo") }
                };

                var model = entity.ToModel();
                model.ApiSecrets.First().Type.Should().Be(def.ApiSecrets.First().Type);
            });
        });
    }
}
