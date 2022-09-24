using Xenial.Identity.Channels.Mail.MailKit.Components;
using Xenial.Identity.Infrastructure.Channels;

namespace Xenial.Identity.Channels.Mail.MailKit;

internal class MailKitChannel : ICommunicationChannel
{
    public object CreateChannelSettings() => new object();
    public Task SetChannelSettings(string channelSettingsJson) => throw new NotImplementedException();
}

public static class CommunicationChannelOptionsExtensions
{
    public static void AddMailKit(this ICommunicationChannelOptions options)
        => options.RegisterChannel<MailKitChannel, MailkitSettingsComponents>(CommunicationChannelType.Email, "MailKit", "E-Mail (MailKit)");
}
