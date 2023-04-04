using AdapterServer.Data;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using TaskQueueing.Jobs;

namespace AdapterServer.Pages.Publication;

public class ManagePublicationViewModel
{
    public string Endpoint { get; set; } = "";

    public string ChannelUri { get; set; } = "/asset-institute/server/pub-sub";
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

    public async Task OpenSession(IChannelManagement channel, IConsumerPublication consumer, IProviderPublication provider, SettingsService settings, string channelName)
    {
        try
        {
            await channel.GetChannel(ChannelUri);
        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
        {
            await channel.CreateChannel<PublicationChannel>(ChannelUri, "Test");
        }

        var consumerSession = await consumer.OpenSession(ChannelUri, Topic);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri);
        ProviderSessionId = providerSession.Id;

        await Save(settings, channelName);

        // Setup recurring tasks!
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        RecurringJob.AddOrUpdate<PubSubConsumerJob<NewStructureAsset>>("PollNewStructureAssets", x => x.PollSubscription(consumerSession.Id, null), Cron.Minutely);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerPublication consumer, IProviderPublication provider, SettingsService settings, string channelName)
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

        RecurringJob.RemoveIfExists("PollNewStructureAssets");
    }

    public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
    {
        try
        {
            await channel.GetChannel(ChannelUri);

            await channel.DeleteChannel(ChannelUri);

        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault)
        {
        }

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await Save(settings, channelName);

        RecurringJob.RemoveIfExists("PollNewStructureAssets");
    }
}
