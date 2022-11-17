using Bogus;

using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using IdentityModel.Client;

using Shouldly;

using Xenial.Identity.Infrastructure;
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

    [Fact]
    public async Task DeleteUserCannotDeleteSelf()
    {
        using var scope = await SetAccessToken();
        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();
        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = DatabaseUpdateHandler.AdminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName} {IdentityServerConstants.StandardScopes.OpenId}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var userInfo = await Fixture.HttpClient.GetUserInfoAsync(new UserInfoRequest()
        {
            Address = disco.UserInfoEndpoint,
            Token = token.AccessToken,
        });

        var id = userInfo.Claims.First(m => m.Type == "sub").Value;

        var result = await Client.DeleteUserAsync(id);

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialBadRequestException>()
        );
    }

    [Fact]
    public async Task DeleteUserCannotNonExisting()
    {
        using var scope = await SetAccessToken();

        var result = await Client.DeleteUserAsync(new Faker().Random.AlphaNumeric(10));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialNotFoundException>()
        );
    }

    [Fact]
    public async Task DeleteUserExisting()
    {
        using var scope = await SetAccessToken();

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        var result = await Client.DeleteUserAsync(user.Id);

        result.Match(
            r => r.Data.Id.ShouldBe(user.Id),
            e => throw e.Exception
        );
    }

    [Fact]
    public async Task AddToRoleAllowsAllRolesWhenAdministrator()
    {
        using var scope = await SetAccessToken();

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in AuthPolicies.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }

        var roles = user.Claims.Where(m => m.Type == "role").Select(m => m.Value).ToArray();

        roles.ShouldBe(AuthPolicies.Roles, ignoreOrder: true);
    }

    [Fact]
    public async Task AddToRoleNotFound()
    {
        using var scope = await SetAccessToken();

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        var result = await Client.AddToRoleAsync(new(user.Id, new Faker().Random.AlphaNumeric(10)));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialNotFoundException>()
        );
    }

    [Fact]
    public async Task AddToRoleWhenOwnRoles()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        foreach (var role in AuthPolicies.UserManagerRoles)
        {
            createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, role))).Unwrap();
        }

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in createrUser.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }

        user.Roles.ShouldBe(createrUser.Roles, ignoreOrder: true);
    }

    [Fact]
    public async Task AddToRoleWhenUserManagerRoles()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, AuthPolicies.UserManagerRoleName))).Unwrap();

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in createrUser.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }

        user.Roles.ShouldBe(createrUser.Roles, ignoreOrder: true);
    }


    [Fact]
    public async Task AddToRoleDuplicateRoles()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, AuthPolicies.UserManagerRoleName))).Unwrap();

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in createrUser.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }

        foreach (var role in createrUser.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }

        user.Roles.ShouldBe(createrUser.Roles, ignoreOrder: true);
    }

    [Fact]
    public async Task AddToRoleForeignRoleFailes()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, AuthPolicies.UserManagerRoleName))).Unwrap();

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        var result = await Client.AddToRoleAsync(new(user.Id, DatabaseUpdateHandler.AdminRoleName));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialBadRequestException>()
        );
    }

    [Fact]
    public async Task RemoveRoleNotFound()
    {
        using var scope = await SetAccessToken();

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        var result = await Client.RemoveFromRoleAsync(new RemoveFromXenialRoleRequest(user.Id, new Faker().Random.AlphaNumeric(10)));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialNotFoundException>()
        );
    }

    [Fact]
    public async Task RemoveFromRoleAllowsAllRolesWhenAdministrator()
    {
        using var scope = await SetAccessToken();

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in AuthPolicies.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }
        foreach (var role in AuthPolicies.Roles)
        {
            user = (await Client.RemoveFromRoleAsync(new(user.Id, role))).Unwrap();
        }
        var roles = user.Claims.Where(m => m.Type == "role").Select(m => m.Value).ToArray();

        roles.ShouldBe(Array.Empty<string>(), ignoreOrder: true);
    }

    [Fact]
    public async Task RemoveFromRoleWhenOwnRoles()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        foreach (var role in AuthPolicies.UserManagerRoles)
        {
            createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, role))).Unwrap();
        }

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        foreach (var role in createrUser.Roles)
        {
            user = (await Client.AddToRoleAsync(new(user.Id, role))).Unwrap();
        }
        foreach (var role in createrUser.Roles)
        {
            user = (await Client.RemoveFromRoleAsync(new(user.Id, role))).Unwrap();
        }
        user.Roles.ShouldBe(Array.Empty<string>(), ignoreOrder: true);
    }

    [Fact]
    public async Task RemoveFromRoleWhenNotInRole()
    {
        using var scope = await SetAccessToken();
        var adminUserName = new Faker().Internet.Email();
        var createrUser = (await Client.CreateUserAsync(new CreateXenialUserRequest(adminUserName, DatabaseUpdateHandler.AdminPassword))).Unwrap();

        foreach (var role in AuthPolicies.UserManagerRoles)
        {
            createrUser = (await Client.AddToRoleAsync(new(createrUser.Id, role))).Unwrap();
        }

        var disco = await Fixture.HttpClient.GetDiscoveryDocumentAsync();

        var token = await Fixture.HttpClient.RequestPasswordTokenAsync(new()
        {
            Address = disco.TokenEndpoint,
            ClientId = "test-client",
            UserName = adminUserName,
            Password = DatabaseUpdateHandler.AdminPassword,
            Scope = $"role {IdentityServerConstants.LocalApi.ScopeName}",
        });

        Fixture.HttpClient.SetBearerToken(token.AccessToken);

        var user = (await Client.CreateUserAsync(new CreateXenialUserRequest(new Faker().Internet.Email()))).Unwrap();

        var result = await Client.RemoveFromRoleAsync(new(user.Id, AuthPolicies.UserManagerManageRoleName));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialValidationException>()
        );
    }

    [Fact]
    public async Task CannotRemoveRolesFromOwnUser()
    {
        using var scope = await SetAccessToken();

        var userId = (await Client.GetUserIdAsync()).Unwrap().Id;

        var result = await Client.RemoveFromRoleAsync(new(userId, DatabaseUpdateHandler.AdminRoleName));

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialBadRequestException>()
        );
    }

    [Fact]
    public async Task AddClaim()
    {
        using var scope = await SetAccessToken();
        var faker = new Faker();

        var email = faker.Internet.Email();
        var user = (await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                email
            )
        )).Unwrap();
        var claimType = faker.Random.AlphaNumeric(100);
        var claimValue = faker.Random.AlphaNumeric(100);
        user = (await Client.AddClaimAsync(
            new AddXenialClaimRequest(
                user.Id,
                new XenialClaim(claimType, claimValue)
            ), CancellationToken.None)
        ).Unwrap();

        user.Claims.ShouldContain(new XenialClaim(claimType, claimValue));
    }

    [Fact]
    public async Task ValidateClaims()
    {
        using var scope = await SetAccessToken();
        var faker = new Faker();

        var email = faker.Internet.Email();
        var user = (await Client.CreateUserAsync(
            new CreateXenialUserRequest(
                email
            )
        )).Unwrap();

        var claimType = faker.Random.AlphaNumeric(300);
        var claimValue = faker.Random.AlphaNumeric(300);

        var result = (await Client.AddClaimAsync(
            new AddXenialClaimRequest(
                user.Id,
                new XenialClaim(claimType, claimValue)
            ), CancellationToken.None)
        );

        result.Match(
            _ => throw new Exception(),
            e => e.Exception.ShouldBeOfType<XenialValidationException>()
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
            AllowedScopes = new[] { "role", IdentityServerConstants.LocalApi.ScopeName, IdentityServerConstants.StandardScopes.OpenId },
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
