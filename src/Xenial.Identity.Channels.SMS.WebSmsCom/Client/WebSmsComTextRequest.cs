using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Xenial.Identity.Channels.Client;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class WebSmsComTextRequest
{
    public string? MessageContent { get; set; }
    /// Max number of messages to be send
    public uint MaxSmsPerMessage { get; set; } = 1;
    /// Test message?
    public bool Test { get; set; } = true;
    /// List of recipients
    public long[] RecipientAddressList { get; set; } = Array.Empty<long>();
    /// Address of sender
    public string SenderAddress { get; set; } = "";
    /// Type of sender address
    public string SenderAddressType { get; set; } = WebSmsComSenderAddressType.NATIONAL;
    /// Send as flash sms?
    public bool SendAsFlashSms { get; set; } = false;
    /// Notification url
    public string NotificationCallbackUrl { get; set; } = "";
    /// User-defined message id
    public string ClientMessageId { get; set; } = "";
    /// Priority
    public uint Priority { get; set; } = 0;
}
