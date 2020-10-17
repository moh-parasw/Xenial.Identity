using System;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [NonPersistent]
    public abstract class XpoIdentityUserClaim<TKey> : XpoIdentityBaseObject
        where TKey : IEquatable<TKey>
    {
        private TKey id;
        private string type;
        private string val;

        public XpoIdentityUserClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key]
        //[Key(AutoGenerate = true)]
        //TODO: Autogenerate for Key
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }

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
    }
}
