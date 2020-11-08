using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("IdentityResources")]
    public class XpoIdentityResource : XpoStorageBaseObject
    {
        private int id;
        private bool enabled = true;
        private string name;
        private string displayName;
        private string description;
        private bool required;
        private bool emphasize;
        private bool showInDiscoveryDocument = true;
        private DateTime created = DateTime.UtcNow;
        private DateTime? updated;
        private bool nonEditable;

        public XpoIdentityResource(Session session) : base(session) { }

        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        [Persistent("Enabled")]
        public bool Enabled { get => enabled; set => SetPropertyValue(ref enabled, value); }
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

        [Persistent("Required")]
        public bool Required { get => required; set => SetPropertyValue(ref required, value); }

        [Persistent("Emphasize")]
        public bool Emphasize { get => emphasize; set => SetPropertyValue(ref emphasize, value); }

        [Persistent("ShowInDiscoveryDocument")]
        public bool ShowInDiscoveryDocument { get => showInDiscoveryDocument; set => SetPropertyValue(ref showInDiscoveryDocument, value); }
        [Association]
        [Aggregated]
        public XPCollection<XpoIdentityResourceClaim> UserClaims => GetCollection<XpoIdentityResourceClaim>();

        [Association]
        [Aggregated]
        public XPCollection<XpoIdentityResourceProperty> Properties => GetCollection<XpoIdentityResourceProperty>();

        [Persistent("Created")]
        public DateTime Created { get => created; set => SetPropertyValue(ref created, value); }

        [Persistent("Updated")]
        public DateTime? Updated { get => updated; set => SetPropertyValue(ref updated, value); }

        [Persistent("NonEditable")]
        public bool NonEditable { get => nonEditable; set => SetPropertyValue(ref nonEditable, value); }
    }
}
