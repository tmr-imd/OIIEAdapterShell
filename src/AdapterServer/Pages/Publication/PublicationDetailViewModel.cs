using AdapterServer.Data;
using AdapterServer.Services;
using Oiie.Settings;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;
using AbstractMessage = TaskQueueing.ObjectModel.Models.AbstractMessage;
using PublishedMessage = TaskQueueing.ObjectModel.Models.Publication;
using TaskQueueing.ObjectModel.Enums;

namespace AdapterServer.Pages.Publication;

public class PublicationDetailViewModel
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/pub-sub";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public Type? DetailComponentType { get; protected set; } = null;
    public IDictionary<string, object> DetailComponentParameters { get; } = new Dictionary<string, object>();

    public PublishedMessage? Message { get; set; } = null;

    public string Topics => string.Join("\n", Message?.Topics ?? Enumerable.Empty<string>());
    public MessageState MessageState => Message is null ? MessageState.Undefined : Message.State;

    public string? RawContent { get; set; } = null;


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

    public virtual async Task Load(IJobContext context, Guid MessageId)
    {
        Message = await PublicationService.GetPublication( context, MessageId );
        RawContent = null;

        if (Message is not null)
        {
            DetailComponentParameters["Message"] = Message;

            RawContent = Message.Content.Deserialize<string>();

            // if (Message.ResponseContent is not null)
            // {
            //     // TODO: COnfirmBOD.
            // }
        }
    }
}
