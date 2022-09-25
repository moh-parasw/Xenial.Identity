using System;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiResourceSecrets")]
    public class XpoApiResourceSecret : XpoStorageBaseObject
    {
        private int id;
        private string description;
        private string val;
        private DateTime? expiration;
        private string type = "SharedSecret";
        private DateTime created = DateTime.UtcNow;
        private XpoApiResource apiResource;

        public XpoApiResourceSecret(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Description")]
        [Size(1000)]
        public string Description { get => description; set => SetPropertyValue(ref description, value); }

        [Persistent("Value")]
        [Size(4000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Value { get => val; set => SetPropertyValue(ref val, value); }

        [Persistent("Expiration")]
        public DateTime? Expiration { get => expiration; set => SetPropertyValue(ref expiration, value); }

        [Persistent("Type")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        [Persistent("Created")]
        public DateTime Created { get => created; set => SetPropertyValue(ref created, value); }

        [Persistent("ApiResourceId")]
        [Association]
        public XpoApiResource ApiResource { get => apiResource; set => SetPropertyValue(ref apiResource, value); }
    }
}
