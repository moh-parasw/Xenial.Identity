using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("PersistedGrants")]
    [Indices(new[] { index1, index2 })]
    public class XpoPersistedGrant
    {
        private const string index1 = nameof(SubjectId) + ";" + nameof(ClientId) + ";" + nameof(Type);
        private const string index2 = nameof(SubjectId) + ";" + nameof(SessionId) + ";" + nameof(Type);

        [Persistent("Key")]
        [Key(AutoGenerate = false)]
        [Size(200)]
        public string Key { get; set; }

        [Persistent("Type")]
        [Size(50)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Type { get; set; }

        [Persistent("SubjectId")]
        [Size(200)]
        public string SubjectId { get; set; }

        [Persistent("SessionId")]
        [Size(100)]
        public string SessionId { get; set; }

        [Persistent("ClientId")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string ClientId { get; set; }

        [Persistent("Description")]
        [Size(200)]
        public string Description { get; set; }

        [Persistent("CreationTime")]
        [RuleRequiredField(DefaultContexts.Save)]
        public DateTime CreationTime { get; set; }

        [Persistent("Expiration")]
        [Indexed]
        public DateTime? Expiration { get; set; }

        [Persistent("ConsumedTime")]
        public DateTime? ConsumedTime { get; set; }

        // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
        // apparently anything over 4K converts to nvarchar(max) on SqlServer
        [Persistent("Data")]
        [Size(50000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Data { get; set; }
    }
}
