using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoClientScope
    {
        public int Id { get; set; }
        public string Scope { get; set; }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get; set; }
    }
}
