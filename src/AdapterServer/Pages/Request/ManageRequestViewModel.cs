using AdapterServer.Data;
using Oiie.Settings;
using Hangfire;
using Isbm2Client.Interface;
using Isbm2Client.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml.Linq;
using TaskQueueing.Data;
using TaskQueueing.Jobs;
using TaskQueueing.Persistence;

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
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public ManageRequestViewModel( NavigationManager navigation, JobContextFactory factory, ClaimsPrincipal principal )
    {
        this.navigation = navigation;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task LoadSettings(SettingsService settings, string channelName)
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

    public async Task SaveSettings(SettingsService settings, string channelName)
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

        var listenerUrl = navigation.ToAbsoluteUri("/api/notifications").AbsoluteUri;

        #if DEBUG
            // Not great, but okay for now...
            listenerUrl = listenerUrl.Replace(navigation.BaseUri, "http://host.docker.internal:5060/");
        #endif

        var consumerSession = await consumer.OpenSession(ChannelUri, listenerUrl);
        ConsumerSessionId = consumerSession.Id;

        // We're cheating for the demo
        var providerSession = await provider.OpenSession(ChannelUri, Topic, listenerUrl);
        ProviderSessionId = providerSession.Id;

        await SaveSettings(settings, channelName);

        // Setup recurring tasks!
        switch (MessageType)
        {
            case MessageTypes.JSON:
                RecurringJob.AddOrUpdate<RequestJobJSON>("CheckForRequests", x => x.CheckForRequests(providerSession.Id, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobJSON>("CheckForResponses", x => x.CheckForResponses(consumerSession.Id, null!), Cron.Hourly);
                break;
            case MessageTypes.ExampleBOD:
                RecurringJob.AddOrUpdate<RequestJobBOD>("CheckForRequests", x => x.CheckForRequests(providerSession.Id, null!), Cron.Hourly);
                RecurringJob.AddOrUpdate<ResponseJobBOD>("CheckForResponses", x => x.CheckForResponses(consumerSession.Id, null!), Cron.Hourly);
                break;
            case MessageTypes.CCOM:
                throw new Exception("Not yet implemented");
        }

        await AddOrUpdateStoredSession();
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

        await DeleteStoredSession();

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await SaveSettings(settings, channelName);
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

        await DeleteStoredSession();

        ConsumerSessionId = "";
        ProviderSessionId = "";

        await SaveSettings(settings, channelName);
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
