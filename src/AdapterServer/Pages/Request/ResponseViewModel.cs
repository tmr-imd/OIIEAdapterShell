using AdapterServer.Data;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;
using RequestMessage = TaskQueueing.ObjectModel.Models.Request;

using CommonBOD;
using Oagis;

namespace AdapterServer.Pages.Request;

using RequestBODType = GenericBodType<GetType, List<StructureAssetsFilter>>;
using ResponseBODType = GenericBodType<ShowType, List<RequestStructures>>;

public class ResponseViewModel
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public string FilterCode { get; set; } = "";
    public string FilterType { get; set; } = "";
    public string FilterLocation { get; set; } = "";
    public string FilterOwner { get; set; } = "";
    public string FilterCondition { get; set; } = "";
    public string FilterInspector { get; set; } = "";

    public RequestMessage? Request { get; set; } = null;

    public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();

    private readonly SettingsService settings;

    public ResponseViewModel(IOptions<ClientConfig> config, SettingsService settings)
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

    public async Task Load(IJobContext context, Guid RequestId)
    {
        Request = await RequestService.GetRequest( context, RequestId );

        if (Request is not null)
        {
            StructureAssets = Enumerable.Empty<StructureAsset>();

            if (Request.MediaType == "application/json")
            {
                DeserializeStructures(Request);
            }
            else if (Request.Content.RootElement.ValueKind == JsonValueKind.String)
            {
                DeserializeBOD(Request);
            }
        }
    }

    private void DeserializeStructures(RequestMessage request)
    {
        var filter = request.Content.Deserialize<StructureAssetsFilter>();

        if (filter is not null)
        {
            FilterCode = filter.FilterCode;
            FilterType = filter.FilterType;
            FilterLocation = filter.FilterLocation;
            FilterOwner = filter.FilterOwner;
            FilterCondition = filter.FilterCondition;
            FilterInspector = filter.FilterInspector;
        }

        if (request.ResponseContent is not null && request.Responses.Last().MediaType == "application/json")
        {
            // Deserialize the content only. Ignore ConfirmBOD as errors have already been attached to the request
            var structures = request.ResponseContent.Deserialize<RequestStructures>();
            StructureAssets = structures?.StructureAssets ?? Enumerable.Empty<StructureAsset>();
        }
    }

    private void DeserializeBOD(RequestMessage request)
    {
        var bod = new RequestBODType("GetStructureAssets", Ccom.Namespace.URI, nounName: "StructureAssetsFilter");
        bod = DeserializeBODContent<RequestBODType, GetType, List<StructureAssetsFilter>>(request.Content, bod);
        if (bod is null) return;

        var filter = bod.DataArea.Noun.First();
        FilterCode = filter.FilterCode;
        FilterType = filter.FilterType;
        FilterLocation = filter.FilterLocation;
        FilterOwner = filter.FilterOwner;
        FilterCondition = filter.FilterCondition;
        FilterInspector = filter.FilterInspector;

        if (request.ResponseContent is not null)
        {
            // Deserialize the content only. Ignore ConfirmBOD as errors have already been attached to the request
            var responseBod = new ResponseBODType("ShowStructureAssets", Ccom.Namespace.URI);
            responseBod = DeserializeBODContent<ResponseBODType, ShowType, List<RequestStructures>>(request.ResponseContent, responseBod);
            if (responseBod is null) return;

            var structures = responseBod.DataArea.Noun.FirstOrDefault();
            StructureAssets = structures?.StructureAssets ?? Enumerable.Empty<StructureAsset>();
        }
    }

    private T? DeserializeBODContent<T, TV, TN>(JsonDocument content, T bod)
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
}
