using System.Linq;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class ApiResourceMappersTests
    {
        public static void Tests() => Describe(nameof(ApiResourceMappers), () =>
        {
            _ = It("Automapper Configuration is valid", () => ApiResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<ApiResourceMapperProfile>());

            _ = It("Can map", () =>
            {
                using var session = new Session();
                var model = new ApiResource();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Should().NotBeNull();
                _ = mappedEntity.Should().NotBeNull();
            });

            _ = It("Properties map", () =>
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

                _ = mappedEntity.Scopes.Count.Should().Be(2);
                var foo1 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo1");
                _ = foo1.Should().NotBeNull();
                var foo2 = mappedEntity.Scopes.FirstOrDefault(x => x.Scope == "foo2");
                _ = foo2.Should().NotBeNull();


                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Description.Should().Be("description");
                _ = mappedModel.DisplayName.Should().Be("displayname");
                _ = mappedModel.Enabled.Should().BeFalse();
                _ = mappedModel.Name.Should().Be("foo");
            });

            _ = It("Missing values should use defaults", () =>
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
                _ = model.ApiSecrets.First().Type.Should().Be(def.ApiSecrets.First().Type);
            });
        });
    }
}
