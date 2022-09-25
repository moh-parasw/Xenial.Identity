using Newtonsoft.Json;

using Xenial.Identity.Channels.Components;

namespace Xenial.Identity.Channels;

internal class AnySmsChannel : ICommunicationChannel
{
    public object CreateChannelSettings() => new AnySmsSettings();
    public object DeserializeChannelSettings(string settings) => LoadChannelSetting(settings);
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
        => options.RegisterChannel<AnySmsChannel, AnySmsSettingsComponents>(
            CommunicationChannelType.Sms,
            "AnySms",
            "SMS (AnySms)");
}
