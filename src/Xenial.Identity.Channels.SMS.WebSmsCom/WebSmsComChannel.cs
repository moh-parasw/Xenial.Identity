using Newtonsoft.Json;

using Xenial.Identity.Channels.Components;

namespace Xenial.Identity.Channels;

internal class WebSmsComChannel : ICommunicationChannel
{
    public object CreateChannelSettings() => new WebSmsComSettings();
    public object LoadChannelSettings(string settings) => LoadChannelSetting(settings);
    private WebSmsComSettings LoadChannelSetting(string settings) => JsonConvert.DeserializeObject<WebSmsComSettings>(settings)!;
    public string SerializeChannelSettings(object settings) => JsonConvert.SerializeObject(settings, Formatting.Indented);

    public Task SetChannelSettings(string channelSettingsJson)
    {
        _ = LoadChannelSetting(channelSettingsJson);

        return Task.CompletedTask;
    }
}

public static class CommunicationChannelOptionsExtensions
{
    public static void AddWebSmsCom(this ICommunicationChannelOptions options)
        => options.RegisterChannel<WebSmsComChannel, WebSmsComSettingsComponents>(
            CommunicationChannelType.Sms,
            "WebSmsCom",
            "SMS (webbms.com)");
}
