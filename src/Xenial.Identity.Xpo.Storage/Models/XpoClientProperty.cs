using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientProperties")]
    public class XpoClientProperty : XpoStorageBaseObject
    {
        private int id;
        private string key;
        private string val;
        private XpoClient client;

        public XpoClientProperty(Session session) : base(session) { }

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

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
