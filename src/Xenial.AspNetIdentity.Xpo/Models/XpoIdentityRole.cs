using System;

using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [NonPersistent]
    public abstract class XpoIdentityRole<TKey> : XpoIdentityBaseObject where TKey : IEquatable<TKey>
    {
        public XpoIdentityRole(Session session) : base(session) { }

        /// <summary>
        /// Gets or sets the primary key for this role.
        /// </summary>
        [Persistent("Id")]
        [Key]
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        [Indexed(Unique = true)]
        [Size(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        [Persistent]
        [Indexed]
        [Size(50)]
        public string NormalizedName { get; set; }

        ///// <summary>
        ///// A random value that should change whenever a role is persisted to the store
        ///// </summary>
        ////TODO: Concurrency
        //public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Returns the name of the role.
        /// </summary>
        /// <returns>The name of the role.</returns>
        public override string ToString() => Name;
    }
}
