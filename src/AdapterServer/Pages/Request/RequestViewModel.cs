﻿using AdapterServer.Data;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Request;

using RequestJobJSON = RequestConsumerJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using RequestJobBOD = RequestConsumerJob<ProcessGetShowStructuresJob, XDocument, XDocument>;

public class RequestViewModel
{
    public string Endpoint { get; set; } = "";
    public string ChannelUri { get; set; } = "/asset-institute/server/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string SessionId { get; set; } = "";

    public MessageTypes MessageType { get; set; } = MessageTypes.JSON;

    public enum MessageTypes
    {
        JSON, ExampleBOD, CCOM
    }

    public string FilterCode { get; set; } = "";
    public string FilterType { get; set; } = "";
    public string FilterLocation { get; set; } = "";
    public string FilterOwner { get; set; } = "";
    public string FilterCondition { get; set; } = "";
    public string FilterInspector { get; set; } = "";

    private readonly SettingsService settings;

    public RequestViewModel( IOptions<ClientConfig> config, SettingsService settings )
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
            MessageType = channelSettings.MessageType switch
            {
                var m when m == MessageTypes.ExampleBOD.ToString() => MessageTypes.ExampleBOD,
                var m when m == MessageTypes.CCOM.ToString() => MessageTypes.CCOM,
                _ => MessageTypes.JSON
            };
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public void Request()
    {
        switch (MessageType)
        {
            case MessageTypes.JSON:
                RequestJSON();
                break;
            case MessageTypes.ExampleBOD:
                RequestExampleBOD();
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
    }

    public void RequestJSON()
    {
        var requestFilter = new StructureAssetsFilter(FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector);

        BackgroundJob.Enqueue<RequestJobJSON>(x => x.PostRequest(SessionId, requestFilter, Topic, null!));
    }

    public void RequestExampleBOD()
    {
        var requestFilter = new StructureAssetsFilter(FilterCode, FilterType, FilterLocation, FilterOwner, FilterCondition, FilterInspector);
        var bod = requestFilter.ToGetStructureAssetsBOD();
        BackgroundJob.Enqueue<RequestJobBOD>(x => x.PostRequest(SessionId, bod, Topic, null!));
    }
}
