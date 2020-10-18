using System;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    public class XpoIdentityUserClaimGuid : XpoIdentityUserClaim<Guid>
    {
        public XpoIdentityUserClaimGuid(Session session) : base(session) { }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserClaimString : XpoIdentityUserClaimNoAuto<string>
    {
        public XpoIdentityUserClaimString(Session session) : base(session) { }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserClaimInt : XpoIdentityUserClaim<int>
    {
        public XpoIdentityUserClaimInt(Session session) : base(session) { }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserClaimNoAuto<TKey> : XpoIdentityUserClaimBase
        where TKey : IEquatable<TKey>
    {
        private TKey id;

        public XpoIdentityUserClaimNoAuto(Session session) : base(session) { }

        [Persistent("Id")]
        [Key]
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserClaim<TKey> : XpoIdentityUserClaimBase
        where TKey : IEquatable<TKey>
    {
        private TKey id;

        public XpoIdentityUserClaim(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public TKey Id { get => id; set => SetPropertyValue(ref id, value); }
    }

    [NonPersistent]
    public abstract class XpoIdentityUserClaimBase : XpoIdentityBaseObject
    {
        private string type;
        private string val;

        public XpoIdentityUserClaimBase(Session session) : base(session) { }

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
