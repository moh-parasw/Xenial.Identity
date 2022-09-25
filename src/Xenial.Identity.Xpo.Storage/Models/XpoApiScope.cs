using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiScopes")]
    public class XpoApiScope : XpoStorageBaseObject
    {
        public XpoApiScope(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get; set; }

        [Persistent("Enabled")]
        public bool Enabled { get; set; } = true;

        [Persistent("Name")]
        [Size(200)]
        [Indexed(Unique = true)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Name { get; set; }

        [Persistent("DisplayName")]
        [Size(200)]
        public string DisplayName { get; set; }

        [Persistent("Description")]
        [Size(1000)]
        public string Description { get; set; }

        [Persistent("Required")]
        public bool Required { get; set; }

        [Persistent("Emphasize")]
        public bool Emphasize { get; set; }

        [Persistent("ShowInDiscoveryDocument")]
        public bool ShowInDiscoveryDocument { get; set; } = true;

        [Association]
        [Aggregated]
        public XPCollection<XpoApiScopeClaim> UserClaims => GetCollection<XpoApiScopeClaim>();

        [Association]
        [Aggregated]
        public XPCollection<XpoApiScopeProperty> Properties => GetCollection<XpoApiScopeProperty>();
    }
}
