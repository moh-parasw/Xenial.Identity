namespace Xenial.Identity.Channels;

public record MailKitSettings
{
    public string Server { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int Port { get; set; } = 465;
    public string FromName { get; set; } = "";
    public string FromEmail { get; set; } = "";
}
