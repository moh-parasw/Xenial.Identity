using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace Xenial.Identity.Infrastructure;

public class RedirectValidator : StrictRedirectUriValidator
{
    public override async Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
    {
        var result = await base.IsRedirectUriValidAsync(requestedUri, client);

        return !result && TryIsLocalhost(requestedUri, out var isLocalhost) ? isLocalhost : result;
    }

    private static bool TryIsLocalhost(string requestedUri, out bool isLocalhost)
    {
        if (Uri.TryCreate(requestedUri, UriKind.Absolute, out var redirectUri))
        {
            isLocalhost = false;

            if (redirectUri.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                isLocalhost = true;
            }
            if (redirectUri.Host.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase))
            {
                isLocalhost = true;
            }

            return true;
        }

        isLocalhost = false;
        return false;
    }

    public override async Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
    {
        var result = await base.IsPostLogoutRedirectUriValidAsync(requestedUri, client);

        return !result && TryIsLocalhost(requestedUri, out var isLocalhost) ? isLocalhost : result;
    }
}
