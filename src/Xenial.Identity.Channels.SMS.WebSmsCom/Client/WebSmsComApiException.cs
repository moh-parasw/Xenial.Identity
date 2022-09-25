namespace Xenial.Identity.Channels.Client;

public class WebSmsComApiException : Exception
{
    public WebSmsComStatusCode StatusCode { get; }

    public string StatusMessage { get; }

    public WebSmsComApiException(string statusMessage, WebSmsComStatusCode statusCode)
        : base(statusMessage) => (StatusCode, StatusMessage) = (statusCode, statusMessage);
}
