using AdapterServer.Data;
using Oiie.Settings;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;
using RequestMessage = TaskQueueing.ObjectModel.Models.Request;

using CommonBOD;
using Oagis;
using AdapterServer.Services;
using TaskQueueing.ObjectModel.Enums;
using TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Request;

public class RequestResponseDetailViewModel
{
    public string Endpoint { get; set; } = "";
    public ChannelSettings ChannelSettings { get; set; } = new ChannelSettings();

    public Type? RequestDetailComponentType { get; protected set; } = null;
    public IDictionary<string, object> RequestDetailComponentParameters { get; } = new Dictionary<string, object>();

    public Type? ResponseDetailComponentType { get; protected set; } = null;
    public IDictionary<string, object> ResponseDetailComponentParameters { get; } = new Dictionary<string, object>();

    public RequestMessage? Request { get; set; } = null;
    public string Topic => Request?.Topic ?? "";
    public MessageState RequestState => Request is null ? MessageState.Undefined : Request.State;
    private Lazy<string> lazyRawRequest;
    public string RequestRawContent => lazyRawRequest.Value;
    public string ResponseRawContent => ExtractRawContent(Request?.Responses.LastOrDefault());

    private readonly SettingsService settings;

    public RequestResponseDetailViewModel(IOptions<ClientConfig> config, SettingsService settings)
    {
        Endpoint = config.Value?.EndPoint ?? "";

        this.settings = settings;

        lazyRawRequest = new(() => ExtractRawContent(Request));
    }

    public async Task LoadSettings(string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelSettings = channelSettings;
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public virtual async Task Load(IJobContext context, Guid requestId)
    {
        Request = await RequestService.GetRequest(context, requestId);
        lazyRawRequest = new Lazy<string>(() => ExtractRawContent(Request));

        if (Request is not null)
        {
            RequestDetailComponentParameters["Message"] = Request;
            ResponseDetailComponentParameters["Message"] = Request;
        }
    }

    public static T? DeserializeBODContent<T, TV, TN>(JsonDocument content, T bod)
        where T : GenericBodType<TV, TN>
        where TV : VerbType, new()
        where TN : class, new()
    {
        var rawContent = content.Deserialize<string>();
        if (rawContent is null || rawContent.Contains("ConfirmBOD")) return default(T);

        T? newBod;
        using (var input = new StringReader(rawContent))
        {
            newBod = bod.CreateSerializer().Deserialize(input) as T;
        }

        return newBod;
    }

    protected static string ExtractRawContent(AbstractMessage? message)
    {
        if (message is null) return "";

        if (message.Content.RootElement.ValueKind == JsonValueKind.String)
        {
            return message.Content.Deserialize<string>() ?? "No content unexpectedly";
        }

        var doc = message.Content;
        var options = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(doc, options);
    }
}
