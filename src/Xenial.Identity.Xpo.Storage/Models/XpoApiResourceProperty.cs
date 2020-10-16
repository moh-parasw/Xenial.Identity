using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiResourceProperties")]
    public class XpoApiResourceProperty : XpoStorageBaseObject
    {
        private int id;
        private string key;
        private string val;
        private XpoApiResource apiResource;

        public XpoApiResourceProperty(Session session) : base(session) { }

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

        [Persistent("ApiResourceId")]
        [Association]
        public XpoApiResource ApiResource { get => apiResource; set => SetPropertyValue(ref apiResource, value); }
    }
}
