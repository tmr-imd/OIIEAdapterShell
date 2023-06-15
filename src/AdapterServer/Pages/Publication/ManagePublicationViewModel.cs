using AdapterServer.Data;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;

namespace AdapterServer.Pages.Publication;

using MessageTypes = PublicationViewModel.MessageTypes;

public class ManagePublicationViewModel
{
    public string Endpoint { get; set; } = "";

    public string ChannelUri { get; set; } = "/asset-institute/server/pub-sub";
    public string Topic { get; set; } = "Test Topic";
    public string ConsumerSessionId { get; set; } = "";
    public string ProviderSessionId { get; set; } = "";
    public string ConfirmationSessionId { get; set; } = "";

    public MessageTypes MessageType { get; set; } = MessageTypes.JSON;

    private readonly NavigationManager navigation;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public ManagePublicationViewModel(NavigationManager navigation, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.navigation = navigation;
        this.factory = factory;
        this.principal = principal;
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

        var listenerUrl = navigation.ToAbsoluteUri("/api/notifications").AbsoluteUri;

        #if DEBUG
            // Not great, but okay for now...
            listenerUrl = listenerUrl.Replace(navigation.BaseUri, "http://host.docker.internal:5060/");
        #endif

        var consumerSession = await consumer.OpenSession(ChannelUri, Topic, listenerUrl);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri);
        ProviderSessionId = providerSession.Id;

        var confirmationSession = await consumer.OpenSession(ChannelUri, "ConfirmBOD", listenerUrl);
        ConfirmationSessionId = confirmationSession.Id;

        await Save(settings, channelName);

        // Setup recurring tasks!
        switch (MessageType)
        {
            case MessageTypes.JSON:
                RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessNewStructuresJob, NewStructureAsset>>("PollNewStructureAssets", x => x.PollSubscription(consumerSession.Id, null!), Cron.Hourly);
                break;
            case MessageTypes.ExampleBOD:
                RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessSyncStructureAssetsJob, System.Xml.Linq.XDocument>>("PollNewStructureAssets", x => x.PollSubscription(consumerSession.Id, null!), Cron.Hourly);
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }
        RecurringJob.AddOrUpdate<PubSubConsumerJob<ProcessConfirmBODJob, string>>("PollConfirmBOD", x => x.PollSubscription(confirmationSession.Id, null!), Cron.Hourly);

        await AddOrUpdateStoredSession();
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

        await DeleteStoredSession();

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

        await DeleteStoredSession();

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await Save(settings, channelName);
    }

    private async Task AddOrUpdateStoredSession()
    {
        using var context = await factory.CreateDbContext(principal);

        var storedConsumerSession = await context.Sessions.Where(x => x.SessionId == ConsumerSessionId).FirstOrDefaultAsync();
        var storedProviderSession = await context.Sessions.Where(x => x.SessionId == ProviderSessionId).FirstOrDefaultAsync();

        if (storedConsumerSession is null)
        {
            storedConsumerSession = new TaskQueueing.ObjectModel.Models.Session(ConsumerSessionId, "CheckForResponses");
            context.Sessions.Add(storedConsumerSession);
        }
        else
        {
            storedConsumerSession = storedConsumerSession with { RecurringJobId = "CheckForResponses" };
        }

        if (storedProviderSession is null)
        {
            storedProviderSession = new TaskQueueing.ObjectModel.Models.Session(ProviderSessionId, "CheckForRequests");
            context.Sessions.Add(storedProviderSession);
        }
        else
        {
            storedProviderSession = storedProviderSession with { RecurringJobId = "CheckForRequests" };
        }

        await context.SaveChangesAsync();
    }

    private async Task DeleteStoredSession()
    {
        using var context = await factory.CreateDbContext(principal);

        var storedConsumerSession = await context.Sessions.Where(x => x.SessionId == ConsumerSessionId).FirstOrDefaultAsync();
        var storedProviderSession = await context.Sessions.Where(x => x.SessionId == ProviderSessionId).FirstOrDefaultAsync();

        if (storedConsumerSession is not null)
        {
            context.Sessions.Remove(storedConsumerSession);
        }

        if (storedProviderSession is not null)
        {
            context.Sessions.Remove(storedProviderSession);
        }

        await context.SaveChangesAsync();
    }
}
