using AdapterServer.Data;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Xml.Linq;
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

    public MessageTypes MessageType { get; set; } = MessageTypes.JSON;

    public enum MessageTypes
    {
        JSON, ExampleBOD, CCOM
    }

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
            MessageType = channelSettings.MessageType switch
            {
                var m when m == MessageTypes.ExampleBOD.ToString() => MessageTypes.ExampleBOD,
                var m when m == MessageTypes.CCOM.ToString() => MessageTypes.CCOM,
                _ => MessageTypes.JSON
            };
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
            .Where( x => (x.State & TaskModels.MessageState.Received) == TaskModels.MessageState.Received )
            .Where( x => (x.State & TaskModels.MessageState.Processed) == TaskModels.MessageState.Processed )
            .Where( x => x.Topics.Contains(Topic) )
            .Select( x => Deserialize(x) )
            .Where( x => x != null )
            .Cast<NewStructureAsset>();
    }

    public void Post()
    {
        switch (MessageType)
        {
            case MessageTypes.JSON:
                PostJSON();
                break;
            case MessageTypes.ExampleBOD:
                PostExampleBOD();
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
    }

    private void PostExampleBOD()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));
        var bod = newStructure.ToSyncStructureAssetsBOD();
        BackgroundJob.Enqueue<PubSubProviderJob<XDocument>>(x => x.PostPublication(SessionId, bod, Topic, null!));
    }

    private void PostJSON()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

        BackgroundJob.Enqueue<PubSubProviderJob<NewStructureAsset>>(x => x.PostPublication(SessionId, newStructure, Topic, null!));
    }

    private NewStructureAsset? Deserialize(TaskModels.Publication message)
    {
        if (message.MediaType == "application/json")
        {
            return DeserializeStructure(message);
        }
        else 
        {
            return DeserializeBOD(message);
        }
    }

    private NewStructureAsset? DeserializeStructure(TaskModels.Publication message)
    {
        return message.Content.Deserialize<NewStructureAsset>();
    }

    private NewStructureAsset? DeserializeBOD(TaskModels.Publication message)
    {
        var RawContent = message.Content.Deserialize<string>();
        if (RawContent is null) return null;

        var bod = new CommonBOD.GenericBodType<Oagis.SyncType, List<StructureAssets>>("SyncStructureAssets", Ccom.Namespace.URI);
        using (var input = new StringReader(RawContent))
        {
            bod = bod.CreateSerializer().Deserialize(input) as CommonBOD.GenericBodType<Oagis.SyncType, List<StructureAssets>>;
        }
        if (bod?.DataArea.Noun.FirstOrDefault()?.StructureAsset.FirstOrDefault() is null) return null;

        return new NewStructureAsset("Sync", bod.DataArea.Noun.First().StructureAsset.First());
    }
}
