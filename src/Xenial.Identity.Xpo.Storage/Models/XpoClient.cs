using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using IdentityServer4.Models;

using Xenial.Identity.Xpo.Storage.ValueConverters;

namespace Xenial.Identity.Xpo.Storage.Models
{
    /// <summary>
    /// Models an OpenID Connect or OAuth2 client
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    [Persistent("Clients")]
    public class XpoClient : XpoStorageBaseObject
    {
        private static readonly Client @default = new Client();
        private string DebuggerDisplay => ClientId ?? $"{{{typeof(Client)}}}";

        public XpoClient(Session session) : base(session) { }

        private int id;
        private bool enabled = @default.Enabled;
        private string clientId;
        private string protocolType = @default.ProtocolType;
        private string clientName;
        private string clientUri;
        private string logoUri;
        private string description;
        private string frontChannelLogoutUri;
        private string backChannelLogoutUri;
        private string clientClaimsPrefix = @default.ClientClaimsPrefix;
        private string pairWiseSubjectSalt;
        private string userCodeType;
        private ICollection<string> allowedIdentityTokenSigningAlgorithms = new HashSet<string>();
        private bool requireClientSecret = @default.RequireClientSecret;
        private bool requireConsent = @default.RequireConsent;
        private bool allowRememberConsent = @default.AllowRememberConsent;
        private bool requirePkce = @default.RequirePkce;
        private bool allowPlainTextPkce = @default.AllowPlainTextPkce;
        private bool requireRequestObject = @default.RequireRequestObject;
        private bool allowAccessTokensViaBrowser = @default.AllowAccessTokensViaBrowser;
        private bool frontChannelLogoutSessionRequired = @default.FrontChannelLogoutSessionRequired;
        private bool backChannelLogoutSessionRequired = @default.BackChannelLogoutSessionRequired;
        private bool allowOfflineAccess = @default.AllowOfflineAccess;
        private bool alwaysIncludeUserClaimsInIdToken = @default.AlwaysIncludeUserClaimsInIdToken;
        private int identityTokenLifetime = @default.IdentityTokenLifetime;
        private int accessTokenLifetime = @default.AccessTokenLifetime;
        private int authorizationCodeLifetime = @default.AuthorizationCodeLifetime;
        private int absoluteRefreshTokenLifetime = @default.AbsoluteRefreshTokenLifetime;
        private int slidingRefreshTokenLifetime = @default.SlidingRefreshTokenLifetime;
        private int? consentLifetime = null;
        private TokenUsage refreshTokenUsage = @default.RefreshTokenUsage;
        private bool updateAccessTokenClaimsOnRefresh = @default.UpdateAccessTokenClaimsOnRefresh;
        private TokenExpiration refreshTokenExpiration = @default.RefreshTokenExpiration;
        private AccessTokenType accessTokenType = @default.AccessTokenType;
        private bool enableLocalLogin = @default.EnableLocalLogin;
        private bool includeJwtId = @default.IncludeJwtId;
        private bool alwaysSendClientClaims = @default.AlwaysSendClientClaims;
        private int? userSsoLifetime;
        private int deviceCodeLifetime = @default.DeviceCodeLifetime;

        #region Fields with mappings

        /// <summary>
        /// Specifies the key in the database
        /// </summary>
        [Persistent("Id")]
        [Key(AutoGenerate = true)]
        public int Id { get => id; set => SetPropertyValue(ref id, value); }

        /// <summary>
        /// Specifies if client is enabled (defaults to <c>true</c>)
        /// </summary>
        [Persistent("Enabled")]
        public bool Enabled { get => enabled; set => SetPropertyValue(ref enabled, value); }

        /// <summary>
        /// Unique ID of the client
        /// </summary>
        [Persistent("ClientId")]
        [Indexed(Unique = true)]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string ClientId { get => clientId; set => SetPropertyValue(ref clientId, value); }

        /// <summary>
        /// Gets or sets the protocol type.
        /// </summary>
        /// <value>
        /// The protocol type.
        /// </value>
        [Persistent("ProtocolType")]
        [Size(200)]
        [RuleRequiredField(DefaultContexts.Save)]
        public string ProtocolType { get => protocolType; set => SetPropertyValue(ref protocolType, value); }

        /// <summary>
        /// Client display name (used for logging and consent screen)
        /// </summary>
        [Persistent("ClientName")]
        [Size(200)]
        public string ClientName { get => clientName; set => SetPropertyValue(ref clientName, value); }

        /// <summary>
        /// URI to further information about client (used on consent screen)
        /// </summary>
        [Persistent("ClientUri")]
        [Size(2000)]
        public string ClientUri { get => clientUri; set => SetPropertyValue(ref clientUri, value); }

        /// <summary>
        /// URI to client logo (used on consent screen)
        /// </summary>
        [Persistent("LogoUri")]
        [Size(2000)]
        public string LogoUri { get => logoUri; set => SetPropertyValue(ref logoUri, value); }

        /// <summary>
        /// Description of the client.
        /// </summary>
        [Persistent("Description")]
        [Size(1000)]
        public string Description { get => description; set => SetPropertyValue(ref description, value); }

        /// <summary>
        /// Specifies logout URI at client for HTTP front-channel based logout.
        /// </summary>
        [Persistent("FrontChannelLogoutUri")]
        [Size(2000)]
        public string FrontChannelLogoutUri { get => frontChannelLogoutUri; set => SetPropertyValue(ref frontChannelLogoutUri, value); }

        /// <summary>
        /// Specifies logout URI at client for HTTP back-channel based logout.
        /// </summary>
        [Persistent("BackChannelLogoutUri")]
        [Size(2000)]
        public string BackChannelLogoutUri { get => backChannelLogoutUri; set => SetPropertyValue(ref backChannelLogoutUri, value); }

        /// <summary>
        /// Gets or sets a value to prefix it on client claim types. Defaults to <c>client_</c>.
        /// </summary>
        /// <value>
        /// Any non empty string if claims should be prefixed with the value; otherwise, <c>null</c>.
        /// </value>
        [Persistent("ClientClaimsPrefix")]
        [Size(200)]
        public string ClientClaimsPrefix { get => clientClaimsPrefix; set => SetPropertyValue(ref clientClaimsPrefix, value); }

        /// <summary>
        /// Gets or sets a salt value used in pair-wise subjectId generation for users of this client.
        /// </summary>
        [Persistent("PairWiseSubjectSalt")]
        [Size(200)]
        public string PairWiseSubjectSalt { get => pairWiseSubjectSalt; set => SetPropertyValue(ref pairWiseSubjectSalt, value); }

        /// <summary>
        /// Gets or sets the type of the device flow user code.
        /// </summary>
        /// <value>
        /// The type of the device flow user code.
        /// </value>
        [Persistent("UserCodeType")]
        [Size(100)]
        public string UserCodeType { get => userCodeType; set => SetPropertyValue(ref userCodeType, value); }

        /// <summary>
        /// Signing algorithm for identity token. If empty, will use the server default signing algorithm.
        /// </summary>
        [Persistent("AllowedIdentityTokenSigningAlgorithms")]
        [Size(100)]
        [ValueConverter(typeof(AllowedSigningAlgorithmsConverter))]
        public ICollection<string> AllowedIdentityTokenSigningAlgorithms { get => allowedIdentityTokenSigningAlgorithms; set => SetPropertyValue(ref allowedIdentityTokenSigningAlgorithms, value); }

        #endregion

        #region Associations

        /// <summary>
        /// Specifies the allowed grant types (legal combinations of AuthorizationCode, Implicit, Hybrid, ResourceOwner, ClientCredentials).
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientGrantType> AllowedGrantTypes => GetCollection<XpoClientGrantType>();

        /// <summary>
        /// Specifies allowed URIs to return tokens or authorization codes to
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientRedirectUri> RedirectUris => GetCollection<XpoClientRedirectUri>();

        /// <summary>
        /// Specifies allowed URIs to redirect to after logout
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientPostLogoutRedirectUri> PostLogoutRedirectUris => GetCollection<XpoClientPostLogoutRedirectUri>();

        /// <summary>
        /// Specifies the api scopes that the client is allowed to request. If empty, the client can't access any scope
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientScope> AllowedScopes => GetCollection<XpoClientScope>();

        /// <summary>
        /// Client secrets - only relevant for flows that require a secret
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientSecret> ClientSecrets => GetCollection<XpoClientSecret>();

        /// <summary>
        /// Allows settings claims for the client (will be included in the access token).
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientClaim> Claims => GetCollection<XpoClientClaim>();

        /// <summary>
        /// Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.
        /// </summary>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientIdPRestriction> IdentityProviderRestrictions => GetCollection<XpoClientIdPRestriction>();

        /// <summary>
        /// Gets or sets the allowed CORS origins for JavaScript clients.
        /// </summary>
        /// <value>
        /// The allowed CORS origins.
        /// </value>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientCorsOrigin> AllowedCorsOrigins => GetCollection<XpoClientCorsOrigin>();

        /// <summary>
        /// Gets or sets the custom properties for the client.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        [Association]
        [Aggregated]
        public XPCollection<XpoClientProperty> Properties => GetCollection<XpoClientProperty>();

        #endregion

        #region Fields without mappings

        /// <summary>
        /// If set to false, no client secret is needed to request tokens at the token endpoint (defaults to <c>true</c>)
        /// </summary>
        [Persistent("RequireClientSecret")]
        public bool RequireClientSecret { get => requireClientSecret; set => SetPropertyValue(ref requireClientSecret, value); }

        /// <summary>
        /// Specifies whether a consent screen is required (defaults to <c>false</c>)
        /// </summary>
        [Persistent("RequireConsent")]
        public bool RequireConsent { get => requireConsent; set => SetPropertyValue(ref requireConsent, value); }

        /// <summary>
        /// Specifies whether user can choose to store consent decisions (defaults to <c>true</c>)
        /// </summary>
        [Persistent("AllowRememberConsent")]
        public bool AllowRememberConsent { get => allowRememberConsent; set => SetPropertyValue(ref allowRememberConsent, value); }

        /// <summary>
        /// Specifies whether a proof key is required for authorization code based token requests (defaults to <c>true</c>).
        /// </summary>
        [Persistent("RequirePkce")]
        public bool RequirePkce { get => requirePkce; set => SetPropertyValue(ref requirePkce, value); }

        /// <summary>
        /// Specifies whether a proof key can be sent using plain method (not recommended and defaults to <c>false</c>.)
        /// </summary>
        [Persistent("AllowPlainTextPkce")]
        public bool AllowPlainTextPkce { get => allowPlainTextPkce; set => SetPropertyValue(ref allowPlainTextPkce, value); }

        /// <summary>
        /// Specifies whether the client must use a request object on authorize requests (defaults to <c>false</c>.)
        /// </summary>
        [Persistent("RequireRequestObject")]
        public bool RequireRequestObject { get => requireRequestObject; set => SetPropertyValue(ref requireRequestObject, value); }

        /// <summary>
        /// Controls whether access tokens are transmitted via the browser for this client (defaults to <c>false</c>).
        /// This can prevent accidental leakage of access tokens when multiple response types are allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> if access tokens can be transmitted via the browser; otherwise, <c>false</c>.
        /// </value>
        [Persistent("AllowAccessTokensViaBrowser")]
        public bool AllowAccessTokensViaBrowser { get => allowAccessTokensViaBrowser; set => SetPropertyValue(ref allowAccessTokensViaBrowser, value); }

        /// <summary>
        /// Specifies is the user's session id should be sent to the FrontChannelLogoutUri. Defaults to <c>true</c>.
        /// </summary>
        [Persistent("FrontChannelLogoutSessionRequired")]
        public bool FrontChannelLogoutSessionRequired { get => frontChannelLogoutSessionRequired; set => SetPropertyValue(ref frontChannelLogoutSessionRequired, value); }

        /// <summary>
        /// Specifies is the user's session id should be sent to the BackChannelLogoutUri. Defaults to <c>true</c>.
        /// </summary>
        [Persistent("BackChannelLogoutSessionRequired")]
        public bool BackChannelLogoutSessionRequired { get => backChannelLogoutSessionRequired; set => SetPropertyValue(ref backChannelLogoutSessionRequired, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [allow offline access]. Defaults to <c>false</c>.
        /// </summary>
        [Persistent("AllowOfflineAccess")]
        public bool AllowOfflineAccess { get => allowOfflineAccess; set => SetPropertyValue(ref allowOfflineAccess, value); }

        /// <summary>
        /// When requesting both an id token and access token, should the user claims always be added to the id token instead of requiring the client to use the userinfo endpoint.
        /// Defaults to <c>false</c>.
        /// </summary>
        [Persistent("AlwaysIncludeUserClaimsInIdToken")]
        public bool AlwaysIncludeUserClaimsInIdToken { get => alwaysIncludeUserClaimsInIdToken; set => SetPropertyValue(ref alwaysIncludeUserClaimsInIdToken, value); }

        /// <summary>
        /// Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        [Persistent("IdentityTokenLifetime")]
        public int IdentityTokenLifetime { get => identityTokenLifetime; set => SetPropertyValue(ref identityTokenLifetime, value); }

        /// <summary>
        /// Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        /// </summary>
        [Persistent("AccessTokenLifetime")]
        public int AccessTokenLifetime { get => accessTokenLifetime; set => SetPropertyValue(ref accessTokenLifetime, value); }

        /// <summary>
        /// Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
        /// </summary>
        [Persistent("AuthorizationCodeLifetime")]
        public int AuthorizationCodeLifetime { get => authorizationCodeLifetime; set => SetPropertyValue(ref authorizationCodeLifetime, value); }

        /// <summary>
        /// Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
        /// </summary>
        [Persistent("AbsoluteRefreshTokenLifetime")]
        public int AbsoluteRefreshTokenLifetime { get => absoluteRefreshTokenLifetime; set => SetPropertyValue(ref absoluteRefreshTokenLifetime, value); }

        /// <summary>
        /// Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
        /// </summary>
        [Persistent("SlidingRefreshTokenLifetime")]
        public int SlidingRefreshTokenLifetime { get => slidingRefreshTokenLifetime; set => SetPropertyValue(ref slidingRefreshTokenLifetime, value); }

        /// <summary>
        /// Lifetime of a user consent in seconds. Defaults to null (no expiration)
        /// </summary>
        [Persistent("ConsentLifetime")]
        public int? ConsentLifetime { get => consentLifetime; set => SetPropertyValue(ref consentLifetime, value); }

        /// <summary>
        /// ReUse: the refresh token handle will stay the same when refreshing tokens
        /// OneTime: the refresh token handle will be updated when refreshing tokens
        /// </summary>
        [Persistent("RefreshTokenUsage")]
        public TokenUsage RefreshTokenUsage { get => refreshTokenUsage; set => SetPropertyValue(ref refreshTokenUsage, value); }

        /// <summary>
        /// Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
        /// Defaults to <c>false</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the token should be updated; otherwise, <c>false</c>.
        /// </value>
        [Persistent("UpdateAccessTokenClaimsOnRefresh")]
        public bool UpdateAccessTokenClaimsOnRefresh { get => updateAccessTokenClaimsOnRefresh; set => SetPropertyValue(ref updateAccessTokenClaimsOnRefresh, value); }

        /// <summary>
        /// Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
        /// Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed AbsoluteRefreshTokenLifetime.
        /// </summary>        
        [Persistent("RefreshTokenExpiration")]
        public TokenExpiration RefreshTokenExpiration { get => refreshTokenExpiration; set => SetPropertyValue(ref refreshTokenExpiration, value); }

        /// <summary>
        /// Specifies whether the access token is a reference token or a self contained JWT token (defaults to Jwt).
        /// </summary>
        [Persistent("AccessTokenType")]
        public AccessTokenType AccessTokenType { get => accessTokenType; set => SetPropertyValue(ref accessTokenType, value); }

        /// <summary>
        /// Gets or sets a value indicating whether the local login is allowed for this client. Defaults to <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if local logins are enabled; otherwise, <c>false</c>.
        /// </value>
        [Persistent("EnableLocalLogin")]
        public bool EnableLocalLogin { get => enableLocalLogin; set => SetPropertyValue(ref enableLocalLogin, value); }

        /// <summary>
        /// Gets or sets a value indicating whether JWT access tokens should include an identifier. Defaults to <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> to add an id; otherwise, <c>false</c>.
        /// </value>
        [Persistent("IncludeJwtId")]
        public bool IncludeJwtId { get => includeJwtId; set => SetPropertyValue(ref includeJwtId, value); }

        /// <summary>
        /// Gets or sets a value indicating whether client claims should be always included in the access tokens - or only for client credentials flow.
        /// Defaults to <c>false</c>
        /// </summary>
        /// <value>
        /// <c>true</c> if claims should always be sent; otherwise, <c>false</c>.
        /// </value>
        [Persistent("AlwaysSendClientClaims")]
        public bool AlwaysSendClientClaims { get => alwaysSendClientClaims; set => SetPropertyValue(ref alwaysSendClientClaims, value); }

        /// <summary>
        /// The maximum duration (in seconds) since the last time the user authenticated.
        /// </summary>
        [Persistent("UserSsoLifetime")]
        public int? UserSsoLifetime { get => userSsoLifetime; set => SetPropertyValue(ref userSsoLifetime, value); }

        /// <summary>
        /// Gets or sets the device code lifetime.
        /// </summary>
        /// <value>
        /// The device code lifetime.
        /// </value>
        [Persistent("DeviceCodeLifetime")]
        public int DeviceCodeLifetime { get => deviceCodeLifetime; set => SetPropertyValue(ref deviceCodeLifetime, value); }

        #endregion
    }
}
