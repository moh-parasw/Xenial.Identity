using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xenial.Identity.Configuration
{
    public static class ClientConstants
    {
        //http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
        public static readonly IList<string> StandardClaims = new[]
        {
            "name",
            "given_name",
            "family_name",
            "middle_name",
            "nickname",
            "preferred_username",
            "profile",
            "picture",
            "website",
            "gender",
            "birthdate",
            "zoneinfo",
            "locale",
            "address",
            "updated_at",
            "email",
            "email_verified",
            "phone",
            "phone_verified",
            "sub",
            "openid",
        };
    }
}
