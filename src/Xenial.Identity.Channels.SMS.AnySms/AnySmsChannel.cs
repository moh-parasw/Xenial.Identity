using Newtonsoft.Json;

using Xenial.Identity.Channels.SMS.AnySms.Components;
using Xenial.Identity.Infrastructure.Channels;

namespace Xenial.Identity.Channels.SMS.AnySms;

internal class AnySmsChannel : ICommunicationChannel
{
    public object CreateChannelSettings() => new AnySmsSettings();
    public object LoadChannelSettings(string settings) => LoadChannelSetting(settings);
    private AnySmsSettings LoadChannelSetting(string settings) => JsonConvert.DeserializeObject<AnySmsSettings>(settings)!;
    public string SerializeChannelSettings(object settings) => JsonConvert.SerializeObject(settings, Formatting.Indented);

    public Task SetChannelSettings(string channelSettingsJson)
    {
        _ = LoadChannelSetting(channelSettingsJson);

        return Task.CompletedTask;
    }

}

public static class CommunicationChannelOptionsExtensions
{
    public static void AddAnySms(this ICommunicationChannelOptions options)
        => options.RegisterChannel<AnySmsChannel, AnySmsSettingsComponents>(CommunicationChannelType.Sms, "AnySms", "SMS (AnySms)");
}
