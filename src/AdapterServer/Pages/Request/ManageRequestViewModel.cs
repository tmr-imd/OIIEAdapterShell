using AdapterServer.Data;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using System.Xml.Linq;
using TaskQueueing.Data;
using TaskQueueing.Jobs;

namespace AdapterServer.Pages.Request;

using RequestJobJSON = RequestProviderJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using ResponseJobJSON = RequestConsumerJob<ProcessStructuresJob, StructureAssetsFilter, RequestStructures>;
using RequestJobBOD = RequestProviderJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using ResponseJobBOD = RequestConsumerJob<ProcessGetShowStructuresJob, XDocument, XDocument>;
using MessageTypes = RequestViewModel.MessageTypes;

public class ManageRequestViewModel
{
    public string Endpoint { get; set; } = "";

    public string ChannelUri { get; set; } = "/asset-institute/demo/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string ConsumerSessionId { get; set; } = "";
    public string ProviderSessionId { get; set; } = "";

    public MessageTypes MessageType { get; set; } = MessageTypes.JSON;

    private readonly NavigationManager navigation;

    public ManageRequestViewModel( NavigationManager navigation )
    {
        this.navigation = navigation;
    }

    public async Task Load(SettingsService settings, string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            ConsumerSessionId = channelSettings.ConsumerSessionId;
            ProviderSessionId = channelSettings.ProviderSessionId;
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

    public async Task Save(SettingsService settings, string channelName)
    {
        var channelSettings = new ChannelSettings
        {
            ChannelUri = ChannelUri,
            Topic = Topic,
            ConsumerSessionId = ConsumerSessionId,
            ProviderSessionId = ProviderSessionId,
            MessageType = MessageType.ToString()
        };

        await settings.SaveSettings(channelSettings, channelName);
    }

    public async Task OpenSession(IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, SettingsService settings, string channelName)
    {
        try
        {
            await channel.GetChannel(ChannelUri);
        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
        {
            await channel.CreateChannel<RequestChannel>(ChannelUri, "Test");
        }

        var consumerListenerUri = MessageType switch
        {
            MessageTypes.JSON => navigation.ToAbsoluteUri("/api/request/consumer/json/notifications").AbsoluteUri,
            MessageTypes.ExampleBOD => navigation.ToAbsoluteUri("/api/request/consumer/examplebod/notifications").AbsoluteUri,
            _ => throw new Exception("Not yet implemented")
        };

        var providerListenerUri = MessageType switch
        {
            MessageTypes.JSON => navigation.ToAbsoluteUri("/api/request/provider/json/notifications").AbsoluteUri,
            MessageTypes.ExampleBOD => navigation.ToAbsoluteUri("/api/request/provider/examplebod/notifications").AbsoluteUri,
            _ => throw new Exception("Not yet implemented")
        };

        #if DEBUG
            // Not great, but okay for now...
            consumerListenerUri = consumerListenerUri.Replace(navigation.BaseUri, "http://host.docker.internal:5060/" );
            providerListenerUri = providerListenerUri.Replace(navigation.BaseUri, "http://host.docker.internal:5060/" );
        #endif

        var consumerSession = await consumer.OpenSession(ChannelUri, consumerListenerUri);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri, Topic, providerListenerUri);
        ProviderSessionId = providerSession.Id;

        await Save(settings, channelName);

        // Setup recurring tasks!
        //switch (MessageType)
        //{
        //    case MessageTypes.JSON:
        //        RecurringJob.AddOrUpdate<RequestJobJSON>("CheckForRequests", x => x.CheckForRequests(providerSession.Id), Cron.Hourly);
        //        RecurringJob.AddOrUpdate<ResponseJobJSON>("CheckForResponses", x => x.CheckForResponses(consumerSession.Id), Cron.Hourly);
        //        break;
        //    case MessageTypes.ExampleBOD:
        //        RecurringJob.AddOrUpdate<RequestJobBOD>("CheckForRequests", x => x.CheckForRequests(providerSession.Id), Cron.Hourly);
        //        RecurringJob.AddOrUpdate<ResponseJobBOD>("CheckForResponses", x => x.CheckForResponses(consumerSession.Id), Cron.Hourly);
        //        break;
        //    case MessageTypes.CCOM:
        //        throw new Exception("Not yet implemented");
        //}
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, SettingsService settings, string channelName)
    {
        RecurringJob.RemoveIfExists("CheckForRequests");
        RecurringJob.RemoveIfExists("CheckForResponses");

        try
        {
            await channel.GetChannel(ChannelUri);

            await consumer.CloseSession(ConsumerSessionId);
            await provider.CloseSession(ProviderSessionId);

        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
        {
        }

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await Save(settings, channelName);
    }

    public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
    {
        RecurringJob.RemoveIfExists("CheckForRequests");
        RecurringJob.RemoveIfExists("CheckForResponses");

        try
        {
            await channel.DeleteChannel(ChannelUri);
        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
        {
        }

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await Save(settings, channelName);
    }
}
