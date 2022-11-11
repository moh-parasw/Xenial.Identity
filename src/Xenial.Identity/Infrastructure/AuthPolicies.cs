using System.Security.Claims;

using Duende.IdentityServer;

using Microsoft.AspNetCore.Authorization;

namespace Xenial.Identity.Infrastructure;

public class AuthPolicies
{
    public const string UsersRead = "users:read";
    public const string UsersCreate = "users:create";
    public const string UsersDelete = "users:delete";
    public const string UsersManage = "users:manage";

    public const string UserManagerRoleName = "UserManager";

    public const string UserManagerReadRoleName = $"{UserManagerRoleName}:read";
    public const string UserManagerCreateRoleName = $"{UserManagerRoleName}:create";
    public const string UserManagerDeleteRoleName = $"{UserManagerRoleName}:delete";
    public const string UserManagerManageRoleName = $"{UserManagerRoleName}:manage";

    public static string[] Roles => new[]
    {
        DatabaseUpdateHandler.AdminRoleName,
    }
    .Concat(UserManagerRoles)
    .ToArray();

    public static readonly string[] UserManagerRoles = new[]
    {
        UserManagerRoleName,
        UserManagerReadRoleName,
        UserManagerCreateRoleName,
        UserManagerDeleteRoleName,
        UserManagerManageRoleName
    };

    internal static void Configure(AuthorizationOptions o)
    {
        var policies = new Dictionary<string, string>
        {
            [UsersRead] = UserManagerReadRoleName,
            [UsersCreate] = UserManagerCreateRoleName,
            [UsersDelete] = UserManagerDeleteRoleName,
            [UsersManage] = UserManagerManageRoleName,
        };

        foreach (var (policyName, roleName) in policies)
        {
            o.AddPolicy(policyName, o =>
            {
                o.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
                o.RequireAuthenticatedUser();
                o.RequireRole(DatabaseUpdateHandler.AdminRoleName, UserManagerRoleName, roleName);
            });
        }
    }

    public static bool IsAllowedToAdd(ClaimsPrincipal user, string roleName)
        => user.IsInRole(UserManagerRoleName) switch
        {
            true => UserManagerRoles.Contains(roleName),
            false => false
        };
}
