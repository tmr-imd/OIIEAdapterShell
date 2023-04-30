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
    public string ConfirmationSessionId { get; set; } = "";

    public bool AsBOD { get; set; } = true;

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

        // Settings for the ConfirmBOD queue
        var confirmationSettings = new ChannelSettings
        {
            ChannelUri = ChannelUri,
            Topic = "ConfirmBOD",
            ConsumerSessionId = ConfirmationSessionId,
            ProviderSessionId = ProviderSessionId
        };

        await settings.SaveSettings(channelSettings, channelName + "-confirm");
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

        var confirmationSession = await consumer.OpenSession(ChannelUri, "ConfirmBOD");
        ConfirmationSessionId = confirmationSession.Id;

        await Save(settings, channelName);

        // Setup recurring tasks!
        if (AsBOD)
        {
            RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessSyncStructureAssetsJob, System.Xml.Linq.XDocument>>("PollNewStructureAssets", x => x.PollSubscription(consumerSession.Id, null!), Cron.Minutely);
        }
        else
        {
            RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessNewStructuresJob, NewStructureAsset>>("PollNewStructureAssets", x => x.PollSubscription(consumerSession.Id, null!), Cron.Minutely);
        }
        RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessConfirmBODJob, string>>("PollConfirmBOD", x => x.PollSubscription(confirmationSession.Id, null!), Cron.Minutely);
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerPublication consumer, IProviderPublication provider, SettingsService settings, string channelName)
    {
        RecurringJob.RemoveIfExists("PollNewStructureAssets");
        RecurringJob.RemoveIfExists("PollConfirmBOD");

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
        RecurringJob.RemoveIfExists("PollNewStructureAssets");
        RecurringJob.RemoveIfExists("PollConfirmBOD");

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
