using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoClientPostLogoutRedirectUri
    {
        public int Id { get; set; }
        public string PostLogoutRedirectUri { get; set; }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get; set; }
    }
}
