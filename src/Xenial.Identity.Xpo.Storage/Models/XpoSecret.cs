using System;
using System.Collections.Generic;
using System.Text;

using IdentityServer4.Models;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoSecret
    {
        private static readonly Secret @default = new Secret();

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; } = @default.Description;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; } = @default.Value;

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        public DateTime? Expiration { get; set; } = @default.Expiration;

        /// <summary>
        /// Gets or sets the type of the client secret.
        /// </summary>
        /// <value>
        /// The type of the client secret.
        /// </value>
        public string Type { get; set; } = @default.Type;
    }
}
