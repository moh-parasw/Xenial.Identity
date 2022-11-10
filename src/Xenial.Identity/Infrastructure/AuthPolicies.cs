using Duende.IdentityServer;

using Microsoft.AspNetCore.Authorization;

namespace Xenial.Identity.Infrastructure;

public class AuthPolicies
{
    public const string UsersRead = "users:read";
    public const string UsersCreate = "users:create";

    public const string UserManagerRoleName = "UserManager";

    public const string UserManagerReadRoleName = $"{UserManagerRoleName}:read";
    public const string UserManagerCreateRoleName = $"{UserManagerRoleName}:create";

    internal static void Configure(AuthorizationOptions o)
    {
        o.AddPolicy(UsersRead, o =>
        {
            o.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
            o.RequireAuthenticatedUser();
            o.RequireRole(DatabaseUpdateHandler.AdminRoleName, UserManagerRoleName, UserManagerReadRoleName);
        });

        o.AddPolicy(UsersCreate, o =>
        {
            o.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
            o.RequireAuthenticatedUser();
            o.RequireRole(DatabaseUpdateHandler.AdminRoleName, UserManagerRoleName, UserManagerCreateRoleName);
        });
    }
}
