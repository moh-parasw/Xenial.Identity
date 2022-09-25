namespace Xenial.Identity.Channels;

public record AnySmsSettings
{
    public string Server { get; set; } = "http://gateway.any-sms.biz/send_sms.php";
    public string AccountId { get; set; } = "";
    public string AccountPassword { get; set; } = "";
    public string AccountGateway { get; set; } = "";
}
