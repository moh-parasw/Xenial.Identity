using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Xenial.Identity.Channels.Client;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class WebSmsComResponse
{
    public WebSmsComStatusCode StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string ClientMessageId { get; set; } = string.Empty;
    public string TransferId { get; set; } = string.Empty;
    public int SmsCount { get; set; }
}
