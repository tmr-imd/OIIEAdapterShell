using AdapterServer.Data;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using TaskQueueing.Data;

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

    public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();

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
            SessionId = channelSettings.ConsumerSessionId;
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public void Clear()
    {
        //StructureAssets = Enumerable.Empty<StructureAsset>();
    }

    public void Post()
    {
        //var newStructure = new NewStructureAsset("Sync", new StructureAsset(Code, Type, Location, Owner, Condition, Inspector));

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

    public record class NewStructureAsset(string Verb, StructureAsset Data);

}
