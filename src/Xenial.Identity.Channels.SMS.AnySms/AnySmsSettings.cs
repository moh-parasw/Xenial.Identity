using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenial.Identity.Channels.SMS.AnySms;

public record AnySmsSettings
{
    public string Server { get; set; } = "http://gateway.any-sms.biz/send_sms.php";
    public string AccountId { get; set; } = "";
    public string AccountPassword { get; set; } = "";
    public string AccountGateway { get; set; } = "";
}
