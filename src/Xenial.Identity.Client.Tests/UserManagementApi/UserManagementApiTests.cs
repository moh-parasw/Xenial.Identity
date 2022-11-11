using Bogus;

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
        using var scope = await SetAccessToken();

        var users = (await Client.GetUsersAsync()).Unwrap();

        users.ShouldNotBeNull();
        users.Count().ShouldBe(1);
    }

    [Fact]
    public async Task CreateUserValidatesEmail()
    {
        using var scope = await SetAccessToken();
        var faker = new Faker();

        var user = await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                faker.Random.AlphaNumeric(10)
            )
        );

        user.Match(
            m => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialValidationException>()
        );
    }

    [Fact]
    public async Task CreateUser()
    {
        using var scope = await SetAccessToken();
        var faker = new Faker();

        var email = faker.Internet.Email();
        var user = (await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                email
            )
        )).Unwrap();

        user.UserName.ShouldBe(email);
    }

    [Fact]
    public async Task DuplicatedUser()
    {
        using var scope = await SetAccessToken();

        var user = await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                DatabaseUpdateHandler.AdminUserName
            )
        );

        user.Match(
            m => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialValidationException>()
        );
    }

    [Fact]
    public async Task CanLoginAfterCreationWithPassword()
    {
        using var scope = await SetAccessToken();
        var faker = new Faker();

        var email = faker.Internet.Email();
        var password = DatabaseUpdateHandler.AdminPassword;
        var user = (await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                email,
                password
            )
        )).Unwrap();

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = email,
            Password = password,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        token.ShouldSatisfyAllConditions(
            () => token.Error.ShouldBeNullOrEmpty(),
            () => token.ErrorDescription.ShouldBeNullOrEmpty()
        );
    }

    private async Task<IServiceScope> SetAccessToken()
    {
        var scope = Fixture.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IClientFactory>();
        await factory.CreateClient(new Duende.IdentityServer.Models.Client
        {
            ClientId = "test-client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            RequireClientSecret = false,
            AllowedScopes = new[] { "role", IdentityServerConstants.LocalApi.ScopeName },
            Enabled = true,
            AlwaysSendClientClaims = true,
            ClientClaimsPrefix = ""
        });

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = DatabaseUpdateHandler.AdminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);
        return scope;
    }
}
