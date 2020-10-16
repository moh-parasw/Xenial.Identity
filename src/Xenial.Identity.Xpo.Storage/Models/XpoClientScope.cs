using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientScopes")]
    public class XpoClientScope : XpoStorageBaseObject
    {
        private int id;
        private string scope;
        private XpoClient client;

        public XpoClientScope(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Scope")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Scope { get => scope; set => SetPropertyValue(ref scope, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
