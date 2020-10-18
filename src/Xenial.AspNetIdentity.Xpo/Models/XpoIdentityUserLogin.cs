using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [NonPersistent]
    public abstract class XpoIdentityUserLogin<TKey> : XpoIdentityBaseObject
        where TKey : IEquatable<TKey>
    {
        private TKey id;
        public XpoIdentityUserLogin(Session session) : base(session) { }

        [Persistent("Id")]
        [Key]
        //[Key(AutoGenerate = true)]
        //TODO: Autogenerate for Key
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }

        /// <summary>
        /// Gets or sets the login provider for the login (e.g. facebook, google)
        /// </summary>
        //TODO: INDEX
        //[Indexed(@"User", Unique = true)]
        [Size(150)]
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the unique provider identifier for this login.
        /// </summary>
        [Persistent]
        [Size(150)]
        public string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets the friendly name used in a UI for this login.
        /// </summary>
        [Persistent]
        [Size(1000)]
        public string ProviderDisplayName { get; set; }

        ///// <summary>
        ///// Gets or sets the primary key of the user associated with this login.
        ///// </summary>
        //public TKey UserId { get; set; }
    }
}
