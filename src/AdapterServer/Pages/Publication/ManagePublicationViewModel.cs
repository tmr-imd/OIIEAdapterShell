using AdapterServer.Shared;
using Oiie.Settings;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.Persistence;
using Microsoft.Extensions.Options;
using AdapterServer.Services;

namespace AdapterServer.Pages.Publication;

public class ManagePublicationViewModel<T> : ManageSessionViewModel where T : struct, System.Enum
{
    public string ConfirmationSessionId { get; set; } = "";

    public T MessageType { get; set; } = Enum.GetValues<T>().First();

    private readonly IScheduledJobsConfig<ManagePublicationViewModel<T>> jobScheduler;

    public ManagePublicationViewModel(NavigationManager navigation, JobContextFactory factory, ClaimsPrincipal principal,
        IScheduledJobsConfig<ManagePublicationViewModel<T>> jobScheduler, IOptions<ClientConfig> isbmClientConfig)
        : base(navigation, factory, principal, isbmClientConfig)
    {
        ChannelUri = "/asset-institute/server/pub-sub";
        this.jobScheduler = jobScheduler;
    }

    public override async Task LoadSettings(SettingsService settings, string channelName)
    {
        try
        {
            var channelSettings = await settings.LoadSettings<PublicationSettings>(channelName);

            ChannelUri = channelSettings.ChannelUri;
            Topic = channelSettings.Topic;
            ConsumerSessionId = channelSettings.ConsumerSessionId;
            ProviderSessionId = channelSettings.ProviderSessionId;
            ConfirmationSessionId = channelSettings.ConfirmationSessionId;
            MessageType = Enum.GetValues<T>().SingleOrDefault(e => e.ToString() == channelSettings.MessageType, Enum.GetValues<T>().First());
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public override async Task SaveSettings(SettingsService settings, string channelName)
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

        var consumerSession = await consumer.OpenSession(ChannelUri, Topic, ListenerUrl);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri);
        ProviderSessionId = providerSession.Id;

        var confirmationSession = await consumer.OpenSession(ChannelUri, "ConfirmBOD", ListenerUrl);
        ConfirmationSessionId = confirmationSession.Id;

        await SaveSettings(settings, channelName);

        // Setup recurring tasks!
        var scheduledJobs = jobScheduler.ScheduleJobs(Topic, ProviderSessionId, ConsumerSessionId, (MessageType, ConfirmationSessionId));

        await AddOrUpdateStoredSession(scheduledJobs);

        // We open both consumer and confirmation sessions, but the scheduler may not actually need both
        // so we close any that we do not actually need.
        await CleanupUnusedSessions(scheduledJobs, consumer);
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerPublication consumer, IProviderPublication provider, SettingsService settings, string channelName)
    {
        using var context = await factory.CreateDbContext(principal);
        var recurringJobId = await PublicationService.JobIdFromSession(ConsumerSessionId, context);
        jobScheduler.UnscheduleJobs(recurringJobId);

        try
        {
            await channel.GetChannel(ChannelUri);

            if (!string.IsNullOrWhiteSpace(ConsumerSessionId))      await consumer.CloseSession(ConsumerSessionId);
            if (!string.IsNullOrWhiteSpace(ProviderSessionId))      await provider.CloseSession(ProviderSessionId);
            if (!string.IsNullOrWhiteSpace(ConfirmationSessionId))  await consumer.CloseSession(ConfirmationSessionId);
        }
        catch (IsbmFault ex) when (ex.FaultType == IsbmFaultType.ChannelFault || ex.FaultType == IsbmFaultType.SessionFault)
        {
        }

        await DeleteStoredSession();

        ConsumerSessionId = "";
        ProviderSessionId = "";
        ConfirmationSessionId = "";

        await SaveSettings(settings, channelName);
    }

    public async Task DestroyChannel(IChannelManagement channel, SettingsService settings, string channelName)
    {
        // TODO: fix this as if there are multiple sessions on a single channel for different things they will not be closed off properly
        jobScheduler.UnscheduleJobs(Topic);

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
        ConfirmationSessionId = "";

        await SaveSettings(settings, channelName);
    }

    protected override async Task DeleteStoredSession()
    {
        await base.DeleteStoredSession();
        using var context = await factory.CreateDbContext(principal);

        var storedConfirmationSession = await context.Sessions.Where(x => x.SessionId == ConfirmationSessionId).FirstOrDefaultAsync();

        if (storedConfirmationSession is not null)
        {
            context.Sessions.Remove(storedConfirmationSession);
        }

        await context.SaveChangesAsync();
    }

    private async Task CleanupUnusedSessions(IDictionary<string, string> scheduledJobs, IConsumerPublication consumer)
    {
        if (!scheduledJobs.ContainsKey(ConsumerSessionId))
        {
            await consumer.CloseSession(ConsumerSessionId);
        }

        if (!scheduledJobs.ContainsKey(ConfirmationSessionId))
        {
            await consumer.CloseSession(ConfirmationSessionId);
        }
    }
}
