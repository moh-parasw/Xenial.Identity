using System;
using System.Security.Claims;

using DevExpress.Xpo;


using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [XPIdentityUser(RoleType = typeof(XpoIdentityRole), ClaimsType = typeof(XpoIdentityUserClaim), LoginsType = typeof(XpoIdentityUserLogin))]
    [Persistent]
    public partial class XpoIdentityUser : XpoIdentityBaseObjectGuid
    {
        public XpoIdentityUser(Session session) : base(session) { }
    }

    [XPIdentityUserClaim(UserType = typeof(XpoIdentityUser))]
    [Persistent]
    public partial class XpoIdentityUserClaim : XpoIdentityBaseObjectGuid
    {
        public XpoIdentityUserClaim(Session session) : base(session) { }
    }

    [XPIdentityUserLogin(UserType = typeof(XpoIdentityUser))]
    [Persistent]
    public partial class XpoIdentityUserLogin : XpoIdentityBaseObjectGuid
    {
        public XpoIdentityUserLogin(Session session) : base(session) { }
    }

 
}
