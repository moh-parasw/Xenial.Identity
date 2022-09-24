using System.Collections.Immutable;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Xenial.Identity.Infrastructure.Channels;

public enum CommunicationChannelType
{
    Email,
    Sms
}

public interface ICommunicationChannel
{
    Task SetChannelSettings(string channelSettingsJson);
}

internal interface ICommunicationChannelRegistry
{
    IEnumerable<ICommunicationChannelRegistration> Registrations { get; }

    ICommunicationChannel GetChannel(CommunicationChannelType channelType, string channelProviderType);

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
}

internal interface ICommunicationChannelRegistration
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

public class CommunicationChannelOptions : ICommunicationChannelOptions
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

        return this;
    }
}

public static class ChannelsServiceCollectionExtension
{
    public static IServiceCollection AddCommunicationChannels(this IServiceCollection services, Action<ICommunicationChannelOptions> options)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = options ?? throw new ArgumentNullException(nameof(options));

        var opt = new CommunicationChannelOptions();

        services.AddSingleton<ICommunicationChannelRegistry, CommunicationChannelRegistry>(s =>
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
                services.AddScoped(typeof(ICommunicationChannel), registration.Channel);
            }

            return registry;
        });

        options.Invoke(opt);

        return services;
    }
}
