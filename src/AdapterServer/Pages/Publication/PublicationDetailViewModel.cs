using AdapterServer.Data;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;
using PublishedMessage = TaskQueueing.ObjectModel.Models.Publication;

namespace AdapterServer.Pages.Publication;

public class PublicationDetailViewModel
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

    public PublishedMessage? Message { get; set; } = null;

    // public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();

    private readonly SettingsService settings;

    public PublicationDetailViewModel(IOptions<ClientConfig> config, SettingsService settings)
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

    public async Task Load(IJobContext context, Guid MessageId)
    {
        Message = await PublicationService.GetPublication( context, MessageId );

        if (Message is not null)
        {
            var filter = Message.Content.Deserialize<NewStructureAsset>();

            if (filter is not null)
            {
                Code = filter.Data.Code;
                Type = filter.Data.Type;
                Location = filter.Data.Location;
                Owner = filter.Data.Owner;
                Condition = filter.Data.Condition;
                Inspector = filter.Data.Inspector;
            }

            // if (Message.ResponseContent is not null)
            // {
            //     // TODO: COnfirmBOD.
            // }
        }
    }
}
