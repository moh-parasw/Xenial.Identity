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
        private string addressCountry;
        private string addressPostalCode;
        private string addressRegion;
        private string addressLocality;
        private string addressStreetAddress2;
        private string addressStreetAddress1;
        private string companyName;
        private string initials;
        private string color;
        private string lastName;
        private string firstName;
        private string fullName;
        private DateTime? updatedAt;

        public XpoXeniaIIdentityUser(Session session) : base(session) { }

        [Persistent]
        [Size(100)]
        public string FullName { get => fullName; set => SetPropertyValue(ref fullName, value); }
        [Persistent]
        [Size(50)]
        public string FirstName { get => firstName; set => SetPropertyValue(ref firstName, value); }
        [Persistent]
        [Size(50)]
        public string LastName { get => lastName; set => SetPropertyValue(ref lastName, value); }
        [Persistent]
        [Size(10)]
        public string Color { get => color; set => SetPropertyValue(ref color, value); }
        [Persistent]
        [Size(4)]
        public string Initials { get => initials; set => SetPropertyValue(ref initials, value); }

        [Persistent]
        [Size(100)]
        public string CompanyName { get => companyName; set => SetPropertyValue(ref companyName, value); }
        [Persistent]
        [Size(250)]
        public string AddressStreetAddress1 { get => addressStreetAddress1; set => SetPropertyValue(ref addressStreetAddress1, value); }
        [Persistent]
        [Size(250)]
        public string AddressStreetAddress2 { get => addressStreetAddress2; set => SetPropertyValue(ref addressStreetAddress2, value); }
        [Persistent]
        [Size(250)]
        public string AddressLocality { get => addressLocality; set => SetPropertyValue(ref addressLocality, value); }
        [Persistent]
        [Size(250)]
        public string AddressRegion { get => addressRegion; set => SetPropertyValue(ref addressRegion, value); }
        [Persistent]
        [Size(10)]
        public string AddressPostalCode { get => addressPostalCode; set => SetPropertyValue(ref addressPostalCode, value); }
        [Persistent]
        [Size(100)]
        public string AddressCountry { get => addressCountry; set => SetPropertyValue(ref addressCountry, value); }

        [Persistent]
        public DateTime? UpdatedAt { get => updatedAt; set => SetPropertyValue(ref updatedAt, value); }
    }
}
