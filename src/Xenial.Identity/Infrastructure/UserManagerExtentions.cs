using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using Xenial.Identity.Data;

namespace Xenial.Identity.Infrastructure
{
    public static class UserManagerExtentions
    {
        public static async Task SetOrUpdateClaimAsync(this UserManager<XenialIdentityUser> userManager, XenialIdentityUser user, Claim newClaim)
        {
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == newClaim.Type))
            {
                var claim = claims.First(c => c.Type == newClaim.Type);
                await userManager.ReplaceClaimAsync(user, claim, newClaim);
            }
            else
            {
                await userManager.AddClaimAsync(user, newClaim);
            }
        }

        public static async Task RemoveClaimAsync(this UserManager<XenialIdentityUser> userManager, XenialIdentityUser user, string claimType)
        {
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Any(c => c.Type == claimType))
            {
                var claim = claims.First(c => c.Type == claimType);
                await userManager.RemoveClaimAsync(user, claim);
            }
        }
    }
}
