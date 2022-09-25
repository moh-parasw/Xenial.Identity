using System;
using System.Linq;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using FluentAssertions;

using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Mappers
{
    public static class ClientMappersTests
    {
        public static void Tests() => Describe(nameof(ClientMappers), () =>
        {
            _ = It("Automapper Configuration is valid", () => ClientMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid<ClientMapperProfile>());

            _ = It("Can map", () =>
            {
                using var session = new Session();
                var model = new Client();
                var mappedEntity = model.ToEntity(session);
                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Should().NotBeNull();
                _ = mappedEntity.Should().NotBeNull();
            });

            _ = It("Properties map", () =>
            {
                using var session = new Session();
                var model = new Client()
                {
                    Properties =
                    {
                        {"foo1", "bar1"},
                        {"foo2", "bar2"},
                    }
                };


                var mappedEntity = model.ToEntity(session);

                _ = mappedEntity.Properties.Count.Should().Be(2);
                var foo1 = mappedEntity.Properties.FirstOrDefault(x => x.Key == "foo1");
                _ = foo1.Should().NotBeNull();
                _ = foo1.Value.Should().Be("bar1");
                var foo2 = mappedEntity.Properties.FirstOrDefault(x => x.Key == "foo2");
                _ = foo2.Should().NotBeNull();
                _ = foo2.Value.Should().Be("bar2");

                var mappedModel = mappedEntity.ToModel();

                _ = mappedModel.Properties.Count.Should().Be(2);
                _ = mappedModel.Properties.ContainsKey("foo1").Should().BeTrue();
                _ = mappedModel.Properties.ContainsKey("foo2").Should().BeTrue();
                _ = mappedModel.Properties["foo1"].Should().Be("bar1");
                _ = mappedModel.Properties["foo2"].Should().Be("bar2");
            });

            _ = It("duplicate properties in db map", () =>
            {
                using var session = new Session();
                var entity = new XpoClient(session)
                {
                    Properties =
                    {
                        new XpoClientProperty(session) { Key = "foo1", Value = "bar1" },
                        new XpoClientProperty(session) { Key = "foo1", Value = "bar2" },
                    }
                };

                Action modelAction = () => entity.ToModel();
                _ = modelAction.Should().Throw<Exception>();
            });

            _ = It("missing values should use defaults", () =>
            {
                using var session = new Session();
                var entity = new XpoClient(session)
                {
                    ClientSecrets =
                    {
                        new XpoClientSecret(session)
                    }
                };

                var def = new Client
                {
                    ClientSecrets = { new Secret("foo") }
                };

                var model = entity.ToModel();
                _ = model.ProtocolType.Should().Be(def.ProtocolType);
                _ = model.ClientSecrets.First().Type.Should().Be(def.ClientSecrets.First().Type);
            });
        });
    }
}
