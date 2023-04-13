using AdapterServer.Data;
using AdapterServer.Pages.Request;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Publication;

public class PublicationViewModel
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/pub-sub";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public string Code { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Condition { get; set; } = "";
    public string Inspector { get; set; } = "";

    public bool Ready { get; set; }

    public IEnumerable<NewStructureAsset> PostedAssets { get; set; } = Enumerable.Empty<NewStructureAsset>();

    private readonly SettingsService settings;
    private readonly PublicationService service;

    public PublicationViewModel(IOptions<ClientConfig> config, SettingsService settings, PublicationService service)
    {
        Endpoint = config.Value?.EndPoint ?? "";
        this.settings = settings;
        this.service = service;
    }

    public async Task LoadSettings(string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            SessionId = channelSettings.ProviderSessionId;
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public async Task Load(IJobContext context)
    {
        IEnumerable<TaskModels.Publication> publications = await service.ListPublications(context);

        PostedAssets = publications
            .Select( x => x.Content.Deserialize<NewStructureAsset>() )
            .Where( x => x != null )
            .Cast<NewStructureAsset>();
    }

    public void Post()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

        BackgroundJob.Enqueue<PubSubProviderJob<NewStructureAsset>>(x => x.PostPublication(SessionId, newStructure, Topic, null!));
    }
}
