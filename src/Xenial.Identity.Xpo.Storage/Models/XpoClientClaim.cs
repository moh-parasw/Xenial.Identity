using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

using IdentityServer4.Models;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoClientClaim
    {
        private static readonly ClientClaim @default = new ClientClaim();

        public int Id { get; set; }

        /// <summary>
        /// The claim type
        /// </summary>
        public string Type { get; set; } = @default.Type;

        /// <summary>
        /// The claim value
        /// </summary>
        public string Value { get; set; } = @default.Value;

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get; set; }
    }
}
