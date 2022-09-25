using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientGrantTypes")]
    public class XpoClientGrantType : XpoStorageBaseObject
    {
        private int id;
        private string grantType;
        private XpoClient client;

        public XpoClientGrantType(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("GrantType")]
        [Size(250)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string GrantType { get => grantType; set => SetPropertyValue(ref grantType, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
