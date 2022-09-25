using System;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using Duende.IdentityServer.Models;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientSecrets")]
    public class XpoClientSecret : XpoStorageBaseObject
    {
        private static readonly Secret @default = new();
        private int id;
        private string description = @default.Description;
        private string val = @default.Value;
        private DateTime? expiration = @default.Expiration;
        private string type = @default.Type;
        private DateTime created = DateTime.UtcNow;
        private XpoClient client;

        public XpoClientSecret(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Persistent("Description")]
        [Size(2000)]
        public string Description { get => description; set => SetPropertyValue(ref description, value); }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [Persistent("Value")]
        [Size(4000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Value { get => val; set => SetPropertyValue(ref val, value); }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        [Persistent("Expiration")]
        public DateTime? Expiration { get => expiration; set => SetPropertyValue(ref expiration, value); }

        /// <summary>
        /// Gets or sets the type of the client secret.
        /// </summary>
        /// <value>
        /// The type of the client secret.
        /// </value>
        [Persistent("Type")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        [Persistent("Created")]
        public DateTime Created { get => created; set => SetPropertyValue(ref created, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
