﻿using System.Collections.Immutable;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Xenial.Identity.Channels;

public enum CommunicationChannelType
{
    Email,
    Sms
}

public interface ICommunicationChannel
{
    object CreateChannelSettings();
    string SerializeChannelSettings(object settings);
    object DeserializeChannelSettings(string channelSettingsJson);
    Task SetChannelSettings(string channelSettingsJson);
}

public interface ICommunicationChannelRegistry
{
    IEnumerable<ICommunicationChannelRegistration> Registrations { get; }

    ICommunicationChannel GetChannel(CommunicationChannelType channelType, string channelProviderType);
    ICommunicationChannelRegistration GetChannelRegistration(CommunicationChannelType channelType, string channelProviderType);
    ICommunicationChannel GetChannel(ICommunicationChannelRegistration registration);

    void AddChannel(ICommunicationChannelRegistration registration);
}

internal record CommunicationChannelRegistry(IServiceProvider Provider) : ICommunicationChannelRegistry
{
    private ImmutableArray<ICommunicationChannelRegistration> registrations = ImmutableArray.Create<ICommunicationChannelRegistration>();
    public IEnumerable<ICommunicationChannelRegistration> Registrations => registrations;

    public void AddChannel(ICommunicationChannelRegistration registration)
    {
        _ = registration ?? throw new ArgumentNullException(nameof(registration));
        registrations = registrations.Add(registration);
    }

    public ICommunicationChannel GetChannel(CommunicationChannelType channelType, string channelProviderType)
    {
        var registration = Registrations.First(m => m.Type == channelType && m.ProviderType == channelProviderType);
        return Provider.GetServices<ICommunicationChannel>().First(m => m.GetType() == registration.ChannelType);
    }

    public ICommunicationChannel GetChannel(ICommunicationChannelRegistration registration)
        => GetChannel(registration.Type, registration.ProviderType);
    public ICommunicationChannelRegistration GetChannelRegistration(
        CommunicationChannelType type,
        string providerType) => Registrations.First(m => m.Type == type && m.ProviderType == providerType);
}

public interface ICommunicationChannelRegistration
{
    CommunicationChannelType Type { get; }
    string ProviderType { get; }
    Type ChannelType { get; }
    Type SettingsComponent { get; }
    string DisplayName { get; }
}

internal record CommunicationChannelRegistration(
    CommunicationChannelType Type,
    string ProviderType,
    string DisplayName,
    Type ChannelType,
    Type SettingsComponent
) : ICommunicationChannelRegistration;

public interface ICommunicationChannelOptions
{
    ICommunicationChannelOptions RegisterChannel<TChannel, TSettingsComponent>(
        CommunicationChannelType channelType,
        string channelProviderType,
        string displayName
    ) where TChannel : class, ICommunicationChannel
      where TSettingsComponent : ComponentBase;
}

public record CommunicationChannelOptions(IServiceCollection Services) : ICommunicationChannelOptions
{
    internal record Registration(
        Type Channel,
        Type SettingsComponent,
        CommunicationChannelType ChannelType,
        string ChannelProviderType,
        string DisplayName
    );

    private ImmutableArray<Registration> registrations = ImmutableArray.Create<Registration>();
    internal IReadOnlyList<Registration> Registrations => registrations;

    ICommunicationChannelOptions ICommunicationChannelOptions.RegisterChannel
        <TChannel, TSettingsComponent>(
            CommunicationChannelType channelType,
            string channelProviderType,
            string displayName
    )
    {
        var registration = new Registration(
                typeof(TChannel),
                typeof(TSettingsComponent),
                channelType,
                channelProviderType,
                displayName);

        registrations = registrations.Add(registration);
        _ = Services.AddTransient<ICommunicationChannel, TChannel>();

        return this;
    }
}

public static class ChannelsServiceCollectionExtension
{
    public static IServiceCollection AddCommunicationChannels(this IServiceCollection services, Action<ICommunicationChannelOptions> options)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = options ?? throw new ArgumentNullException(nameof(options));

        var opt = new CommunicationChannelOptions(services);

        _ = services.AddSingleton<ICommunicationChannelRegistry, CommunicationChannelRegistry>(s =>
        {
            var registry = new CommunicationChannelRegistry(s);

            foreach (var registration in opt.Registrations)
            {
                registry.AddChannel(new CommunicationChannelRegistration(
                    registration.ChannelType,
                    registration.ChannelProviderType,
                    registration.DisplayName,
                    registration.Channel,
                    registration.SettingsComponent
                ));
            }

            return registry;
        });

        options.Invoke(opt);

        return services;
    }
}
