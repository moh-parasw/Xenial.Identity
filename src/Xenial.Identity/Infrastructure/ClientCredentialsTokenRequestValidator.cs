using Duende.IdentityServer;
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

        var scopes = context.Result.ValidatedRequest.RequestedScopes
            ?? context.Result.ValidatedRequest.Client.AllowedScopes
            ?? Enumerable.Empty<string>();

        //TODO: Configure via DB
        if (scopes.Contains(IdentityServerConstants.LocalApi.ScopeName))
        {
            var ctx = new ProfileDataRequestContext(
                context.Result.ValidatedRequest.Subject,
                context.Result.ValidatedRequest.Client,
                GetType().Name,
                scopes
            );

            await profileService.GetProfileDataAsync(ctx);

            foreach (var claim in ctx.IssuedClaims)
            {
                context.Result.ValidatedRequest.ClientClaims.Add(claim);
            }
        }
    }
}
