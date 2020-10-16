using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientRedirectUris")]
    public class XpoClientRedirectUri : XpoStorageBaseObject
    {
        private int id;
        private string redirectUri;
        private XpoClient client;

        public XpoClientRedirectUri(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("RedirectUri")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Size(2000)]
        public string RedirectUri { get => redirectUri; set => SetPropertyValue(ref redirectUri, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
