using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Xpo;

using Xenial.AspNetIdentity.Xpo.Models;

namespace Xenial.Identity.Models
{
    [MapInheritance(MapInheritanceType.ParentTable)]
    public class XpoXeniaIIdentityUser : XpoIdentityUser
    {
        public XpoXeniaIIdentityUser(Session session) : base(session) { }
    }
}
