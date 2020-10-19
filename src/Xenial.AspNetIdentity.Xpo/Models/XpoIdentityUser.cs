using System;
using System.Security.Claims;

using DevExpress.Xpo;


using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [XPIdentityUser(
        RoleType = typeof(XpoIdentityRole),
        ClaimsType = typeof(XpoIdentityUserClaim),
        LoginsType = typeof(XpoIdentityUserLogin),
        TokensType = typeof(XpoIdentityUserToken)
    )]
    [Persistent]
    public partial class XpoIdentityUser : XpoIdentityBaseObjectString
    {
        public XpoIdentityUser(Session session) : base(session) { }
    }

    [XPIdentityUserClaim(UserType = typeof(XpoIdentityUser))]
    [Persistent]
    public partial class XpoIdentityUserClaim : XpoIdentityBaseObjectString
    {
        public XpoIdentityUserClaim(Session session) : base(session) { }
    }

    [XPIdentityUserLogin(UserType = typeof(XpoIdentityUser))]
    [Persistent]
    public partial class XpoIdentityUserLogin : XpoIdentityBaseObjectString
    {
        public XpoIdentityUserLogin(Session session) : base(session) { }
    }

    [XPIdentityUserToken(UserType = typeof(XpoIdentityUser))]
    [Persistent]
    public partial class XpoIdentityUserToken : XpoIdentityBaseObjectString
    {
        public XpoIdentityUserToken(Session session) : base(session) { }
    }
}
