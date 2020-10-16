using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    public class XpoClientProperty
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        [Persistent("ClientId")]
        [Association]
        public XpoClient Client { get; set; }
    }
}
