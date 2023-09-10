using AdapterServer.Data;
using AdapterServer.Services;
using Oiie.Settings;
using AdapterServer.Pages.Request;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Components.Publications;

public class PublicationListViewModel
{
    public IEnumerable<TaskModels.Publication> Publications { get; set; } = Enumerable.Empty<TaskModels.Publication>();

    private readonly SettingsService settings;
    private readonly PublicationService service;

    public PublicationListViewModel(IOptions<ClientConfig> config, SettingsService settings, PublicationService service)
    {
        this.settings = settings;
        this.service = service;
    }

    public async Task LoadSettings(string channelName)
    {
        try
        {
            // TODO: settings for SignalR topic for performing updates?
            await Task.Yield();
            // var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public async Task Load(IJobContext context)
    {
        Publications = await service.ListPublications(context);
    }

    public void Update()
    {
        // TODO: SignalR hook to reload publications list and trigger UI update?
    }
}
