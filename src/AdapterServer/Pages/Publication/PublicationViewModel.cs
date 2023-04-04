using AdapterServer.Data;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;

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

    public IEnumerable<NewStructureAsset> NewStructureAssets { get; set; } = Enumerable.Empty<NewStructureAsset>();

    private readonly SettingsService settings;

    public PublicationViewModel(IOptions<ClientConfig> config, SettingsService settings)
    {
        Endpoint = config.Value?.EndPoint ?? "";
        this.settings = settings;
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
        await Task.Yield();
        //NewStructureAssets = await service.ListRequests(context);
    }

    public void Post()
    {
        var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        BackgroundJob.Enqueue<PubSubProviderJob<NewStructureAsset>>(x => x.PostPublication(SessionId, newStructure, Topic, null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //var message = await provider.PostPublication(providerSession.Id, newStructure, Topic);

        //return message.Id;
    }

    //public async Task<NewStructureAsset> Read()
    //{
    //    var message = await consumer.ReadPublication(consumerSession.Id);
    //    if (message == null) throw new Exception("No message");
    //    await consumer.RemovePublication(consumerSession.Id);

    //    var newStructure = message.MessageContent.Deserialise<NewStructureAsset>();
    //    StructureAssets = StructureAssets.Append(newStructure.Data);

    //    return newStructure;
    //}
}
