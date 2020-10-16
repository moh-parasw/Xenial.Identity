using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Xenial.Identity.Xpo.Storage.Models
{
    [Persistent("DeviceCodes")]
    public class XpoDeviceFlowCodes : XpoStorageBaseObject
    {
        private string deviceCode;
        private string userCode;
        private string subjectId;
        private string sessionId;
        private string clientId;
        private string description;
        private DateTime creationTime;
        private DateTime? expiration;
        private string data;

        public XpoDeviceFlowCodes(Session session) : base(session) { }

        /// <summary>
        /// Gets or sets the device code.
        /// </summary>
        /// <value>
        /// The device code.
        /// </value>
        [Persistent("DeviceCode")]
        [Key(AutoGenerate = false)]
        [Indexed(Unique = true)]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string DeviceCode { get => deviceCode; set => SetPropertyValue(ref deviceCode, value); }

        /// <summary>
        /// Gets or sets the user code.
        /// </summary>
        /// <value>
        /// The user code.
        /// </value>
        [Persistent("UserCode")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string UserCode { get => userCode; set => SetPropertyValue(ref userCode, value); }

        /// <summary>
        /// Gets or sets the subject identifier.
        /// </summary>
        /// <value>
        /// The subject identifier.
        /// </value>
        [Persistent("SubjectId")]
        [Size(200)]
        public string SubjectId { get => subjectId; set => SetPropertyValue(ref subjectId, value); }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        [Persistent("SessionId")]
        [Size(100)]
        public string SessionId { get => sessionId; set => SetPropertyValue(ref sessionId, value); }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [Persistent("ClientId")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string ClientId { get => clientId; set => SetPropertyValue(ref clientId, value); }

        /// <summary>
        /// Gets the description the user assigned to the device being authorized.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Persistent("Description")]
        [Size(200)]
        public string Description { get => description; set => SetPropertyValue(ref description, value); }

        /// <summary>
        /// Gets or sets the creation time.
        /// </summary>
        /// <value>
        /// The creation time.
        /// </value>
        [Persistent("CreationTime")]
        [RuleRequiredField(DefaultContexts.Save)]
        public DateTime CreationTime { get => creationTime; set => SetPropertyValue(ref creationTime, value); }

        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        /// <value>
        /// The expiration.
        /// </value>
        [Persistent("Expiration")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Indexed]
        public DateTime? Expiration { get => expiration; set => SetPropertyValue(ref expiration, value); }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [Persistent("Data")]
        // 50000 chosen to be explicit to allow enough size to avoid truncation, yet stay beneath the MySql row size limit of ~65K
        // apparently anything over 4K converts to nvarchar(max) on SqlServer
        [Size(50000)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Data { get => data; set => SetPropertyValue(ref data, value); }
    }
}
