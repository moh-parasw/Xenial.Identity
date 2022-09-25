namespace Xenial.Identity.Channels.Client;

public enum WebSmsComStatusCode
{
    /// Message sent
    OK = 2000,
    /// Message queued
    OK_QUEUED = 2001,
    /// Test ok
    OK_TEST = 2002,
    /// Malformed XML/JSON
    MALFORMED_XML = 4000,
    /// Invalid credentials
    INVALID_CREDENTIALS = 4001,
    /// At least one recipient address is invalid
    INVALID_RECIPIENT = 4002,
    /// Invalid sender
    INVALID_SENDER = 4003,
    /// Invalid message type
    INVALID_MESSAGE_TYPE = 4004,
    /// Invalid message alphabet
    INVALID_MESSAGE_ALPHABET = 4005,
    /// Invalid message class
    INVALID_MESSAGE_CLASS = 4006,
    /// Invalid message data
    INVALID_MESSAGE_DATA = 4007,
    /// Invalid message id
    INVALID_MESSAGE_ID = 4008,
    /// Invalid text
    INVALID_TEXT = 4009,
    /// Invalid autosegment
    INVALID_AUTOSEGMENT = 4010,
    /// Invalid COD
    INVALID_COD = 4011,
    /// Throttling. Allowed message rate exceeded.
    THROTTLING = 4012,
    /// The allowed message limit exceeded.
    MSG_LIMIT_EXCEEDED = 4013,
    /// Unauthorized IP
    UNAUTHORIZED_IP = 4014,
    /// Invalid priority
    INVALID_MESSAGE_PRIORITY = 4015,
    /// Invalid COD return address
    INVALID_COD_RETURNADDRESS = 4016,
    /// Message would generate multiple segments, but option is not enabled.
    MULTISEGMENTS = 4017,
    /// Method not allowed
    API_METHOD_FORBIDDEN = 4018,
    /// Parameter missing
    PARAMETER_MISSING = 4019,
    // Invalid API key
    INVALID_API_KEY = 4020,
    /// Invalid auth token
    INVALID_AUTH_TOKEN = 4021,
    /// Access denied
    ACCESS_DENIED = 4022,
    /// Rate Limit Exceeded, spam check
    THROTTLING_SPAMMING_IP = 4023,
    /// Too many requests
    THROTTLING_TOO_MANY_REQUESTS = 4024,
    // Too many recipients
    THROTTLING_TOO_MANY_RECIPIENTS = 4025,
    /// Message content too long
    MAX_SMS_PER_MESSAGE_EXCEEDED = 4026,
    /// Internal error
    INTERNAL_ERROR = 5000,
    /// Service not available
    SERVICE_UNAVAILABLE = 5003,
}
