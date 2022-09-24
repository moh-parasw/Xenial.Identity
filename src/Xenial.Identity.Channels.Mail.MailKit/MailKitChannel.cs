using Newtonsoft.Json;

using Xenial.Identity.Channels.Mail.MailKit.Components;
using Xenial.Identity.Infrastructure.Channels;

namespace Xenial.Identity.Channels.Mail.MailKit;

internal class MailKitChannel : ICommunicationChannel
{
    public object CreateChannelSettings() => new MailKitSettings();
    public object LoadChannelSettings(string settings) => LoadChannelSetting(settings);
    private MailKitSettings LoadChannelSetting(string settings) => JsonConvert.DeserializeObject<MailKitSettings>(settings)!;
    public string SerializeChannelSettings(object settings) => JsonConvert.SerializeObject(settings, Formatting.Indented);

    public Task SetChannelSettings(string channelSettingsJson)
    {
        _ = LoadChannelSetting(channelSettingsJson);

        return Task.CompletedTask;
    }

}

public static class CommunicationChannelOptionsExtensions
{
    public static void AddMailKit(this ICommunicationChannelOptions options)
        => options.RegisterChannel<MailKitChannel, MailkitSettingsComponents>(CommunicationChannelType.Email, "MailKit", "E-Mail (MailKit)");
}
