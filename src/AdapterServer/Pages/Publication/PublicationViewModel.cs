using AdapterServer.Data;
using AdapterServer.Services;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Xml.Linq;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;
using TaskEnums = TaskQueueing.ObjectModel.Enums;

namespace AdapterServer.Pages.Publication;

public class PublicationViewModel : AbstractPublicationViewModel<PublicationViewModel.MessageTypes>
{
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

    public IEnumerable<NewStructureAsset> PostedAssets { get; set; } = Enumerable.Empty<NewStructureAsset>();

    public PublicationViewModel(IOptions<ClientConfig> config, SettingsService settings, PublicationService service)
        : base(config, settings, service)
    { }

    public override async Task Load(IJobContext context)
    {
        IEnumerable<TaskModels.Publication> publications = await service.ListPublications(context);

        PostedAssets = publications
            .Where( x => (x.State & TaskEnums.MessageState.Received) == TaskEnums.MessageState.Received )
            .Where( x => (x.State & TaskEnums.MessageState.Processed) == TaskEnums.MessageState.Processed )
            .Where( x => x.Topics.Contains(Topic) )
            .Select( x => Deserialize(x) )
            .Where( x => x != null )
            .Cast<NewStructureAsset>();
    }

    public override void Post()
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
        BackgroundJob.Enqueue<PubSubProviderJob<XDocument>>(x => x.PostPublication(ProviderSessionId, bod, Topic, null!));
    }

    private void PostJSON()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

        BackgroundJob.Enqueue<PubSubProviderJob<NewStructureAsset>>(x => x.PostPublication(ProviderSessionId, newStructure, Topic, null!));
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
