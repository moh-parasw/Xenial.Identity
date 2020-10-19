
using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [XPIdentityRole(UserType = typeof(XpoIdentityUser), ClaimsType = typeof(XpoIdentityRoleClaim))]
    [Persistent]
    public partial class XpoIdentityRole : XpoIdentityBaseObjectString
    {
        public XpoIdentityRole(Session session) : base(session) { }
    }

    [XPIdentityRoleClaim(RoleType = typeof(XpoIdentityRole))]
    [Persistent]
    public partial class XpoIdentityRoleClaim : XpoIdentityBaseObjectString
    {
        public XpoIdentityRoleClaim(Session session) : base(session) { }
    }
}
