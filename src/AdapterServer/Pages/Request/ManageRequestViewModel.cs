using AdapterServer.Shared;
using Oiie.Settings;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskQueueing.Persistence;
using Microsoft.Extensions.Options;

namespace AdapterServer.Pages.Request;

using MessageTypes = RequestViewModel.MessageTypes;

public class ManageRequestViewModel : ManageSessionViewModel
{
    public MessageTypes MessageType { get; set; } = MessageTypes.JSON;

    private readonly IScheduledJobsConfig<ManageRequestViewModel> jobScheduler;

    public ManageRequestViewModel( NavigationManager navigation, JobContextFactory factory, ClaimsPrincipal principal,
        IScheduledJobsConfig<ManageRequestViewModel> jobScheduler, IOptions<ClientConfig> isbmClientConfig)
        : base(navigation, factory, principal, isbmClientConfig)
    {
        ChannelUri = "/asset-institute/demo/request-response";
        this.jobScheduler = jobScheduler;
    }

    public override async Task LoadSettings(SettingsService settings, string channelName)
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

    public override async Task SaveSettings(SettingsService settings, string channelName)
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
            await channel.CreateChannel<RequestChannel>(ChannelUri, Topic);
        }

        var consumerSession = await consumer.OpenSession(ChannelUri, ListenerUrl);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri, Topic, ListenerUrl);
        ProviderSessionId = providerSession.Id;

        await SaveSettings(settings, channelName);

        // Setup recurring tasks!
        var scheduledJobs = jobScheduler.ScheduleJobs(Topic, providerSession.Id, consumerSession.Id, MessageType);

        await AddOrUpdateStoredSession(scheduledJobs);
    }

    public async Task CloseSession(IChannelManagement channel, IConsumerRequest consumer, IProviderRequest provider, SettingsService settings, string channelName)
    {
        jobScheduler.UnscheduleJobs(Topic);

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

        await SaveSettings(settings, channelName);
    }

    protected override async Task DeleteStoredSession()
    {
        await base.DeleteStoredSession();
        using var context = await factory.CreateDbContext(principal);

        var storedProviderSession = await context.Sessions.Where(x => x.SessionId == ProviderSessionId).FirstOrDefaultAsync();

        if (storedProviderSession is not null)
        {
            context.Sessions.Remove(storedProviderSession);
        }

        await context.SaveChangesAsync();
    }
}
