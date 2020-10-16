using System;
using System.Collections.Generic;
using System.Text;

using IdentityServer4.Models;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoClientClaim
    {
        private static readonly ClientClaim @default = new ClientClaim();
        /// <summary>
        /// The claim type
        /// </summary>
        public string Type { get; set; } = @default.Type;

        /// <summary>
        /// The claim value
        /// </summary>
        public string Value { get; set; } = @default.Value;

        /// <summary>
        /// The claim value type
        /// </summary>
        public string ValueType { get; set; } = @default.ValueType;
    }
}
