using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiScopeProperties")]
    public class XpoApiScopeProperty : XpoStorageBaseObject
    {
        private int id;
        private string key;
        private string val;
        private XpoApiScope scope;

        public XpoApiScopeProperty(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Key")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Key { get => key; set => SetPropertyValue(ref key, value); }

        [Persistent("Value")]
        [Size(2000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Value { get => val; set => SetPropertyValue(ref val, value); }

        [Persistent("ScopeId")]
        [Association]
        public XpoApiScope Scope { get => scope; set => SetPropertyValue(ref scope, value); }
    }
}
