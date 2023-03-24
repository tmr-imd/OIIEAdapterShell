using AdapterServer.Data;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Isbm2RestClient.Model;
using Microsoft.Extensions.Options;

namespace AdapterServer.Pages;

public class RequestViewModel : IAsyncDisposable
{
    public string Endpoint { get; set; } = "";
    //public string ChannelUri { get; set; } = "/fred";
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

    //private RequestChannel requestChannel = null!;
    //private RequestConsumerSession consumerSession = null!;
    private readonly IConsumerRequest consumer;

    public RequestViewModel( IOptions<ClientConfig> config, SettingsService settings, IConsumerRequest consumer)
    {
        Endpoint = config.Value?.EndPoint ?? "";

        this.settings = settings;
        this.consumer = consumer;
    }

    public async ValueTask DisposeAsync() => await Teardown();

    public async Task Setup( string channelName )
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

        //try
        //{
        //    requestChannel = await channelManagement.CreateChannel<RequestChannel>(ChannelUri, "Test");
        //}
        //catch (IsbmFault e) when (e.FaultType == IsbmFaultType.ChannelFault)
        //{
        //    await channelManagement.DeleteChannel(ChannelUri);

        //    requestChannel = await channelManagement.CreateChannel<RequestChannel>(ChannelUri, "Test");
        //}

        //consumerSession = await consumer.OpenSession(ChannelUri);
    }

    private async Task Teardown()
    {
        await Task.Yield();
        //if ( consumer != null && consumerSession != null ) await consumer.CloseSession(consumerSession.Id);

        //if ( requestChannel != null ) await channelManagement.DeleteChannel( requestChannel.Uri );
    }

    public void Clear()
    {
        StructureAssets = Enumerable.Empty<StructureAsset>();
    }

    public async Task Process()
    {
        var requestId = await Request();

        StructureAssets = await ReadResponse( requestId );
    }

    public async Task<string> Request()
    {
        var requestFilter = new StructureAssetsFilter( FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector );

        var request = await consumer.PostRequest( SessionId, requestFilter, Topic );

        return request.Id;
    }

    public async Task<IEnumerable<StructureAsset>> ReadResponse( string requestId )
    {
        var message = await consumer.ReadResponse(SessionId, requestId );
        if (message is null) throw new Exception("There was no Response to read");

        await consumer.RemoveResponse(SessionId, requestId );

        var payload = message.MessageContent.Deserialise<RequestStructures>();

        return payload.StructureAssets;
    }
}
