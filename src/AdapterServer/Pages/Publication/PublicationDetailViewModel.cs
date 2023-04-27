using AdapterServer.Data;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;
using AbstractMessage = TaskQueueing.ObjectModel.Models.AbstractMessage;
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

    public string? RawContent { get; set; } = null;

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
        RawContent = null;

        if (Message is not null)
        {
            if (Message.MediaType == "application/json")
            {
                DeserializeStructure(Message);
            }
            else
            {
                RawContent = DeserializeConfirmBOD(Message);
            }

            // if (Message.ResponseContent is not null)
            // {
            //     // TODO: COnfirmBOD.
            // }
        }
    }

    private void DeserializeStructure(PublishedMessage message)
    {
        var structure = message.Content.Deserialize<NewStructureAsset>();

        if (structure is not null)
        {
            Code = structure.Data.Code;
            Type = structure.Data.Type;
            Location = structure.Data.Location;
            Owner = structure.Data.Owner;
            Condition = structure.Data.Condition;
            Inspector = structure.Data.Inspector;
        }
    }

    private string? DeserializeConfirmBOD(AbstractMessage message)
    {
        var bod = message.Content.Deserialize<string>();

        return bod;
    }
}
