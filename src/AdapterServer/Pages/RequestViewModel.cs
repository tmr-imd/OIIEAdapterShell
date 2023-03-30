using AdapterServer.Data;
using Hangfire;
using Hangfire.Storage;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using TaskQueueing;

namespace AdapterServer.Pages;

public class RequestViewModel
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/demo/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public string FilterCode { get; set; } = "";
    public string FilterType { get; set; } = "";
    public string FilterLocation { get; set; } = "";
    public string FilterOwner { get; set; } = "";
    public string FilterCondition { get; set; } = "";
    public string FilterInspector { get; set; } = "";

    public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();

    private readonly SettingsService settings;

    public RequestViewModel( IOptions<ClientConfig> config, SettingsService settings)
    {
        Endpoint = config.Value?.EndPoint ?? "";

        this.settings = settings;
    }

    public async Task Load( string channelName )
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            SessionId = channelSettings.SessionId;
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public void Clear()
    {
        StructureAssets = Enumerable.Empty<StructureAsset>();
    }

    public void Request()
    {
        var requestFilter = new StructureAssetsFilter( FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector );

        BackgroundJob.Enqueue<ConsumerJob>( x => x.PostRequest(SessionId, requestFilter, Topic) );

        //var storage = JobStorage.Current;
        //var api = storage.GetMonitoringApi();
        //var queues = api.Queues();
        //foreach (var queue in queues)
        //{
        //    var jobs = api.FetchedJobs(queue.Name, 0, 100);
        //    Console.WriteLine(jobs.Count().ToString());
        //}
        //var jobs = conn.GetRecurringJobs();

    }

    //public async Task<IEnumerable<StructureAsset>> ReadResponse( string requestId )
    //{
    //    var message = await consumer.ReadResponse(SessionId, requestId );
    //    if (message is null) throw new Exception("There was no Response to read");

    //    await consumer.RemoveResponse(SessionId, requestId );

    //    var payload = message.MessageContent.Deserialise<RequestStructures>();

    //    return payload.StructureAssets;
    //}
}
