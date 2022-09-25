using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiScopeClaims")]
    public class XpoApiScopeClaim : XpoStorageBaseObject
    {
        private int id;
        private string type;
        private XpoApiScope scope;

        public XpoApiScopeClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Type")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        [Persistent("ScopeId")]
        [Association]
        public XpoApiScope Scope { get => scope; set => SetPropertyValue(ref scope, value); }
    }
}
