using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("IdentityResourceClaims")]
    public class XpoIdentityResourceClaim : XpoStorageBaseObject
    {
        private int id;
        private string type;
        private XpoIdentityResource identityResource;

        public XpoIdentityResourceClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Type")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        [Persistent("IdentityResourceId")]
        [Association]
        public XpoIdentityResource IdentityResource { get => identityResource; set => SetPropertyValue(ref identityResource, value); }
    }
}
