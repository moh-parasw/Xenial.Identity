using System;

using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [NonPersistent]
    public abstract class XpoIdentityBaseObjectGuid : XpoIdentityBaseObject
    {
        private Guid id;

        public XpoIdentityBaseObjectGuid(Session session) : base(session) { }

        [Persistent]
        [Key(AutoGenerate = true)]
        public Guid Id { get => id; set => SetPropertyValue(ref id, value); }
    }
}
