using DevExpress.Xpo;

namespace Xenial.AspNetIdentity.Xpo.Models
{
    [NonPersistent]
    public abstract class XpoIdentityBaseObjectString : XpoIdentityBaseObject
    {
        private string id;

        public XpoIdentityBaseObjectString(Session session) : base(session) { }

        [Persistent]
        [Key]
        public string Id { get => id; set => SetPropertyValue(ref id, value); }
    }
}
