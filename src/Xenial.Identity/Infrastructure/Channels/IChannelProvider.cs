using Microsoft.AspNetCore.Components;

using Xenial.Identity.Models;

namespace Xenial.Identity.Infrastructure.Channels;

public interface ICommunicationChannel
{
    Task SetChannelSettings(string channelSettingsJson);
}

internal interface ICommunicationChannelRegistry
{
    internal IEnumerable<ICommunicationChannelRegistration> Registrations { get; }

    ICommunicationChannel GetChannel(CommunicationChannelType channelType, string channelProviderType);

    void AddChannel(ICommunicationChannelRegistration registration);
}

internal interface ICommunicationChannelRegistration
{
    string ProviderType { get; set; }
    CommunicationChannelType Type { get; set; }
    Type ChannelType { get; }
    Type SettingsComponent { get; }
    string DisplayName { get; set; }
}

public interface ICommunicationChannelOptions
{
    ICommunicationChannelOptions RegisterChannel<TChannel, TSettingsComponent>(
        CommunicationChannelType channelType,
        string channelProviderType,
        string displayName
    ) where TChannel : class, ICommunicationChannel
      where TSettingsComponent : ComponentBase;
}
