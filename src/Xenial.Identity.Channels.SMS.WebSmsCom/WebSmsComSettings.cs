namespace Xenial.Identity.Channels;

public record WebSmsComSettings
{
    public string Server { get; set; } = "https://api.websms.com/rest/";

    public bool UseBasicAuth { get; set; } = true;

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ApiKey { get; set; } = "";
}
