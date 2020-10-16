using System;
using System.Collections.Generic;
using System.Text;

using Xenial.Identity.Xpo.Storage.Models;

namespace Xenial.Identity.Xpo.Storage
{
    public static class IdentityXpoTypes
    {
        public static Type[] PersistentTypes = new[]
        {
            typeof(XpoStorageBaseObject),

            typeof(XpoApiResource),
            typeof(XpoApiResourceClaim),
            typeof(XpoApiResourceProperty),
            typeof(XpoApiResourceScope),
            typeof(XpoApiResourceSecret),

            typeof(XpoApiScope),
            typeof(XpoApiScopeClaim),
            typeof(XpoApiScopeProperty),

            typeof(XpoClient),
            typeof(XpoClientClaim),
            typeof(XpoClientCorsOrigin),
            typeof(XpoClientGrantType),
            typeof(XpoClientIdPRestriction),
            typeof(XpoClientPostLogoutRedirectUri),
            typeof(XpoClientProperty),
            typeof(XpoClientRedirectUri),
            typeof(XpoClientScope),
            typeof(XpoClientSecret),

            typeof(XpoDeviceFlowCodes),

            typeof(XpoIdentityResource),
            typeof(XpoIdentityResourceClaim),
            typeof(XpoIdentityResourceProperty),

            typeof(XpoPersistedGrant),
        };
    }
}
