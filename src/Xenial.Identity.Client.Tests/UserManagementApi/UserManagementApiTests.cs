using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using IdentityModel.Client;

using Shouldly;

using Xenial.Identity.Xpo.Storage.Stores;

namespace Xenial.Identity.Client.Tests.UserManagementApi;

public sealed record UserManagementApiTests()
    : XenialIdentityApiBaseTest(new ApplicationInMemoryFixture() with { AllowAutoRedirect = false })
{
    [Fact]
    public async Task GetUsersReturnsAllWhenAdmin()
    {
        using var scope = Fixture.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IClientFactory>();
        await factory.CreateClient(new Duende.IdentityServer.Models.Client
        {
            ClientId = "test-client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            RequireClientSecret = false,
            AllowedScopes = new[] { IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Email, "role", IdentityServerConstants.LocalApi.ScopeName },
            Enabled = true,
            AlwaysSendClientClaims = true,
            ClientClaimsPrefix = ""
        });

        var dic = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = dic.TokenEndpoint,
            ClientId = "test-client",
            UserName = DatabaseUpdateHandler.AdminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"email openid role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        token.IsError.ShouldBeFalse();
        token.AccessToken.ShouldNotBeNull();
        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var users = (await Client.GetUsersAsync()).Unwrap();

        users.ShouldNotBeNull();
        users.Count().ShouldBe(1);
    }
}
