using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenial.Identity.Channels.Mail.MailKit;

public record MailKitSettings
{
    public string Server { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int Port { get; set; } = 465;
    public string FromName { get; set; } = "";
    public string FromEmail { get; set; } = "";
}
