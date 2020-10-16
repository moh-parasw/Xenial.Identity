using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ClientPostLogoutRedirectUris")]
    public class XpoClientPostLogoutRedirectUri : XpoStorageBaseObject
    {
        private int id;
        private string postLogoutRedirectUri;
        private XpoClient client;

        public XpoClientPostLogoutRedirectUri(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("PostLogoutRedirectUri")]
        [Size(2000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string PostLogoutRedirectUri { get => postLogoutRedirectUri; set => SetPropertyValue(ref postLogoutRedirectUri, value); }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get => client; set => SetPropertyValue(ref client, value); }
    }
}
