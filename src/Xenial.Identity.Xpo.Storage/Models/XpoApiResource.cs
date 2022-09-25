using System;
using System.Collections.Generic;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using Xenial.Identity.Xpo.Storage.ValueConverters;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("ApiResources")]
    public class XpoApiResource : XpoStorageBaseObject
    {
        private int id;
        private bool enabled = true;
        private bool required = true;
        private string name;
        private string displayName;
        private string description;
        private ICollection<string> allowedAccessTokenSigningAlgorithms = new HashSet<string>();
        private bool showInDiscoveryDocument = true;
        private DateTime created = DateTime.UtcNow;
        private DateTime? updated;
        private DateTime? lastAccessed;
        private bool nonEditable;

        public XpoApiResource(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Enabled")]
        public bool Enabled { get => enabled; set => SetPropertyValue(ref enabled, value); }

        [Persistent("Required")]
        public bool Required { get => required; set => SetPropertyValue(ref required, value); }

        [Persistent("Name")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        [Indexed(Unique = true)]
        public string Name { get => name; set => SetPropertyValue(ref name, value); }

        [Persistent("DisplayName")]
        [Size(200)]
        public string DisplayName { get => displayName; set => SetPropertyValue(ref displayName, value); }

        [Persistent("Description")]
        [Size(1000)]
        public string Description { get => description; set => SetPropertyValue(ref description, value); }

        [ValueConverter(typeof(AllowedSigningAlgorithmsConverter))]
        [Persistent("AllowedAccessTokenSigningAlgorithms")]
        [Size(100)]
        public ICollection<string> AllowedAccessTokenSigningAlgorithms { get => allowedAccessTokenSigningAlgorithms; set => SetPropertyValue(ref allowedAccessTokenSigningAlgorithms, value); }

        [Persistent("ShowInDiscoveryDocument")]
        public bool ShowInDiscoveryDocument { get => showInDiscoveryDocument; set => SetPropertyValue(ref showInDiscoveryDocument, value); }

        [Association]
        [Aggregated]
        public XPCollection<XpoApiResourceSecret> Secrets => GetCollection<XpoApiResourceSecret>();

        [Association]
        [Aggregated]
        public XPCollection<XpoApiResourceScope> Scopes => GetCollection<XpoApiResourceScope>();

        [Association]
        [Aggregated]
        public XPCollection<XpoApiResourceClaim> UserClaims => GetCollection<XpoApiResourceClaim>();

        [Association]
        [Aggregated]
        public XPCollection<XpoApiResourceProperty> Properties => GetCollection<XpoApiResourceProperty>();

        [Persistent("Created")]
        public DateTime Created { get => created; set => SetPropertyValue(ref created, value); }

        [Persistent("Updated")]
        public DateTime? Updated { get => updated; set => SetPropertyValue(ref updated, value); }

        [Persistent("LastAccessed")]
        public DateTime? LastAccessed { get => lastAccessed; set => SetPropertyValue(ref lastAccessed, value); }

        [Persistent("NonEditable")]
        public bool NonEditable { get => nonEditable; set => SetPropertyValue(ref nonEditable, value); }
    }
}
