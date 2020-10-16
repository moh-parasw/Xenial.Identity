using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using IdentityServer4.Models;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientClaims")]
    public class XpoClientClaim : XpoStorageBaseObject
    {
        private static readonly ClientClaim @default = new ClientClaim();
        private int id;
        private string type = @default.Type;
        private string val = @default.Value;
        private XpoClient client;

        public XpoClientClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        /// <summary>
        /// The claim type
        /// </summary>
        [Persistent("Type")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get => type; set => SetPropertyValue(ref type, value); }

        /// <summary>
        /// The claim value
        /// </summary>
        [Persistent("Value")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Value { get => val; set => SetPropertyValue(ref val, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
