using AdapterServer.Data;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TaskQueueing.Data;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages;

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

    public IEnumerable<StructureAsset> StructureAssets { get; set; } = Enumerable.Empty<StructureAsset>();

    private readonly SettingsService settings;

    public ResponseViewModel( IOptions<ClientConfig> config, SettingsService settings )
    {
        Endpoint = config.Value?.EndPoint ?? "";

        this.settings = settings;
    }

    public async Task LoadSettings( string channelName )
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

    public async Task Load( IJobContext context, string RequestId )
    {
        var request = await context.Requests.Where( x => x.RequestId == RequestId ).FirstOrDefaultAsync();

        if ( request is not null ) 
        {
            var filter = JsonSerializer.Deserialize<StructureAssetsFilter>( request.Filter );

            if ( filter is not null )
            {
                FilterCode = filter.FilterCode;
                FilterType = filter.FilterType;
                FilterLocation = filter.FilterLocation;
                FilterOwner = filter.FilterOwner;
                FilterCondition = filter.FilterCondition;
                FilterInspector = filter.FilterInspector;
            }

            if ( request.Content is not null )
            {
                var structures = request.Content.Deserialize<RequestStructures>();

                if ( structures is not null )
                {
                    StructureAssets = structures.StructureAssets;
                }
            }
        }
    }
}
