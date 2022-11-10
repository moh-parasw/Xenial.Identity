using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;

namespace Xenial.Identity.Infrastructure;

public sealed class ClientCredentialsTokenRequestValidator : ICustomTokenRequestValidator
{
    private readonly IProfileService profileService;
    public ClientCredentialsTokenRequestValidator(IProfileService profileService)
        => this.profileService = profileService;

    public async Task ValidateAsync(CustomTokenRequestValidationContext context)
    {
        if (context.Result.IsError)
        {
            return;
        }
        if (context.Result.ValidatedRequest.Client.AllowedGrantTypes.Any(x => GrantTypes.ResourceOwnerPasswordAndClientCredentials.Contains(x)))
        {
            var ctx = new ProfileDataRequestContext(
                context.Result.ValidatedRequest.Subject,
                context.Result.ValidatedRequest.Client,
                GetType().Name,
                context.Result.ValidatedRequest.RequestedScopes
            );

            await profileService.GetProfileDataAsync(ctx);

            foreach (var claim in ctx.IssuedClaims)
            {
                context.Result.ValidatedRequest.ClientClaims.Add(claim);
            }
        }
    }
}
