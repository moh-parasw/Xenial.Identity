using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientIdPRestrictions")]
    public class XpoClientIdPRestriction : XpoStorageBaseObject
    {
        private int id;
        private string provider;
        private XpoClient client;

        public XpoClientIdPRestriction(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Provider")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Provider { get => provider; set => SetPropertyValue(ref provider, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
