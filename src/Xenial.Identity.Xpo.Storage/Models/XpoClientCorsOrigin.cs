using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientCorsOrigins")]
    public class XpoClientCorsOrigin : XpoStorageBaseObject
    {
        private int id;
        private string origin;
        private XpoClient client;

        public XpoClientCorsOrigin(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Origin")]
        [Size(150)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Origin { get => origin; set => SetPropertyValue(ref origin, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
