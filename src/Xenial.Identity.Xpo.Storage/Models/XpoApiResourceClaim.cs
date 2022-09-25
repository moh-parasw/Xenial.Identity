using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiResourceClaims")]
    public class XpoApiResourceClaim : XpoStorageBaseObject
    {
        private int id;
        private string type;
        private XpoApiResource apiResource;

        public XpoApiResourceClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Type")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        [Persistent("ApiResourceId")]
        [Association]
        public XpoApiResource ApiResource { get => apiResource; set => SetPropertyValue(ref apiResource, value); }
    }
}
