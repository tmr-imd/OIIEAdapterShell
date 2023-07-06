using AdapterServer.Shared;
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
    private readonly IScheduledJobsConfig<ManagePublicationViewModel> jobScheduler;

    public ManagePublicationViewModel(NavigationManager navigation, JobContextFactory factory, ClaimsPrincipal principal,
        IScheduledJobsConfig<ManagePublicationViewModel> jobScheduler)
    {
        this.navigation = navigation;
        this.factory = factory;
        this.principal = principal;
        this.jobScheduler = jobScheduler;
    }

    public async Task Load(SettingsService settings, string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<PublicationSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            ConsumerSessionId = channelSettings.ConsumerSessionId;
            ProviderSessionId = channelSettings.ProviderSessionId;
            ConfirmationSessionId = channelSettings.ConfirmationSessionId;
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
        var channelSettings = new PublicationSettings
        {
            ChannelUri = ChannelUri,
            Topic = Topic,
            ConsumerSessionId = ConsumerSessionId,
            ProviderSessionId = ProviderSessionId,
            ConfirmationSessionId = ConfirmationSessionId,
            MessageType = MessageType.ToString()
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
        jobScheduler.ScheduleJobs(Topic, ProviderSessionId, ConsumerSessionId, (MessageType, ConfirmationSessionId));

        await AddOrUpdateStoredSession();
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerPublication consumer, IProviderPublication provider, SettingsService settings, string channelName)
    {
        jobScheduler.UnscheduleJobs();

        try
        {
            await channel.GetChannel(ChannelUri);

            await consumer.CloseSession(ConsumerSessionId);
            await provider.CloseSession(ProviderSessionId);

        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault || ex.FaultType == IsbmFaultType.SessionFault)
        {
        }

        await DeleteStoredSession();

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await Save(settings, channelName);
    }

    public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
    {
        jobScheduler.UnscheduleJobs();

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
        var storedConfirmationSession = await context.Sessions.Where(x => x.SessionId == ConfirmationSessionId).FirstOrDefaultAsync();

        if (storedConsumerSession is null)
        {
            storedConsumerSession = new TaskQueueing.ObjectModel.Models.Session(ConsumerSessionId, "PollNewStructureAssets");
            context.Sessions.Add(storedConsumerSession);
        }
        else
        {
            storedConsumerSession = storedConsumerSession with { RecurringJobId = "PollNewStructureAssets" };
        }

        if (storedConfirmationSession is null)
        {
            storedConfirmationSession = new TaskQueueing.ObjectModel.Models.Session(ConfirmationSessionId, "PollConfirmBOD");
            context.Sessions.Add(storedConfirmationSession);
        }
        else
        {
            storedConfirmationSession = storedConfirmationSession with { RecurringJobId = "PollConfirmBOD" };
        }

        await context.SaveChangesAsync();
    }

    private async Task DeleteStoredSession()
    {
        using var context = await factory.CreateDbContext(principal);

        var storedConsumerSession = await context.Sessions.Where(x => x.SessionId == ConsumerSessionId).FirstOrDefaultAsync();
        var storedConfirmationSession = await context.Sessions.Where(x => x.SessionId == ConfirmationSessionId).FirstOrDefaultAsync();

        if (storedConsumerSession is not null)
        {
            context.Sessions.Remove(storedConsumerSession);
        }

        if (storedConfirmationSession is not null)
        {
            context.Sessions.Remove(storedConfirmationSession);
        }

        await context.SaveChangesAsync();
    }
}
