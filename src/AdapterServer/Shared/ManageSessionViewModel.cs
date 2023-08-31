
using System.Security.Claims;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Oiie.Settings;
using TaskQueueing.Persistence;

namespace AdapterServer.Shared;

public abstract class ManageSessionViewModel
{
    public string Endpoint { get; set; } = "";
    protected string listenerUrlBase = "";
    public string ListenerUrl => string.IsNullOrWhiteSpace(listenerUrlBase) ?
        navigation.ToAbsoluteUri("/api/notifications").AbsoluteUri
        : new Uri($"{(listenerUrlBase.EndsWith('/') ? listenerUrlBase[..^1] : listenerUrlBase)}/api/notifications").AbsoluteUri;

    public string ChannelUri { get; set; } = "/example/channel";
    public string Topic { get; set; } = "Test Topic";
    public string ConsumerSessionId { get; set; } = "";
    public string ProviderSessionId { get; set; } = "";

    protected readonly NavigationManager navigation;
    protected readonly JobContextFactory factory;
    protected readonly ClaimsPrincipal principal;

    public ManageSessionViewModel(NavigationManager navigation, JobContextFactory factory,
        ClaimsPrincipal principal, IOptions<ClientConfig> isbmClientConfig)
    {
        this.navigation = navigation;
        this.factory = factory;
        this.principal = principal;
        this.Endpoint = isbmClientConfig.Value.EndPoint;
        this.listenerUrlBase = isbmClientConfig.Value.ListenerUrlBase;
    }

    public abstract Task LoadSettings(SettingsService settings, string channelName);
    public abstract Task SaveSettings(SettingsService settings, string channelName);

}