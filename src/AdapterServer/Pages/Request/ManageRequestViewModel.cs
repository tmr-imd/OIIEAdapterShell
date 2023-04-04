using AdapterServer.Data;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.EntityFrameworkCore;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace AdapterServer.Pages.Request;

public class ManageRequestViewModel
{
    public string Endpoint { get; set; } = "";

    public string ChannelUri { get; set; } = "/asset-institute/demo/request-response";
    public string Topic { get; set; } = "Test Topic";
    public string ConsumerSessionId { get; set; } = "";
    public string ProviderSessionId { get; set; } = "";

    public async Task Load(SettingsService settings, string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            ConsumerSessionId = channelSettings.ConsumerSessionId;
            ProviderSessionId = channelSettings.ProviderSessionId;
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
            ProviderSessionId = ProviderSessionId
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

        var consumerSession = await consumer.OpenSession(ChannelUri);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri, Topic);
        ProviderSessionId = providerSession.Id;

        await Save(settings, channelName);

        // Setup recurring tasks!
        RecurringJob.AddOrUpdate<RequestProviderJob<StructureAssetsFilter>>("CheckForRequests", x => x.CheckForRequests(providerSession.Id), Cron.Minutely);
        RecurringJob.AddOrUpdate<RequestConsumerJob>("CheckForResponses", x => x.CheckForResponses(consumerSession.Id), Cron.Minutely);
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, SettingsService settings, string channelName)
    {
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

        RecurringJob.RemoveIfExists("CheckForRequests");
        RecurringJob.RemoveIfExists("CheckForResponses");
    }

    public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
    {
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

        RecurringJob.RemoveIfExists("CheckForRequests");
        RecurringJob.RemoveIfExists("CheckForResponses");
    }
}
