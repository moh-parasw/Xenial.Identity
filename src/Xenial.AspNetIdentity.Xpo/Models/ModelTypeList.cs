using System;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    public static class IdentityModelTypeList
    {
        public static Type[] ModelTypes = new[]
        {
            typeof(XpoIdentityBaseObject),
            typeof(XpoIdentityBaseObjectString),

            typeof(XpoIdentityUser),
            typeof(XpoIdentityUserToken),
            typeof(XpoIdentityUserLogin),
            typeof(XpoIdentityUserClaim),

            typeof(XpoIdentityRole),
            typeof(XpoIdentityRoleClaim),
        };
    }
}
