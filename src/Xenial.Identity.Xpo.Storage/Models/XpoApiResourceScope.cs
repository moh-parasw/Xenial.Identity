using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiResourceScopes")]
    public class XpoApiResourceScope : XpoStorageBaseObject
    {
        private int id;
        private string scope;
        private XpoApiResource apiResource;

        public XpoApiResourceScope(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Scope")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Scope { get => scope; set => SetPropertyValue(ref scope, value); }

        [Persistent("ApiResourceId")]
        [Association]
        public XpoApiResource ApiResource { get => apiResource; set => SetPropertyValue(ref apiResource, value); }
    }
}
