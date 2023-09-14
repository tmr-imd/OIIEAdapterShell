using AdapterServer.Data;
using AdapterServer.Services;
using Oiie.Settings;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;
using System.Security.Claims;

namespace AdapterServer.Components.Publications;

public class PublicationListViewModel
{
    public bool IncludeReceivedMessages { get; set; } = true;
    public bool IncludePostedMessages { get; set; } = true;

    public bool ReceivedMessagesOnly => IncludeReceivedMessages && !IncludePostedMessages;
    public bool PostedMessagesOnly => !IncludeReceivedMessages && IncludePostedMessages;
    public bool PostedAndReceivedMessages => IncludeReceivedMessages && IncludePostedMessages;

    private readonly SettingsService settings;
    private readonly PublicationService service;
    private readonly ILogger<PublicationListViewModel> logger;
    private readonly JobContextFactory factory;
    private readonly ClaimsPrincipal principal;

    public PublicationListViewModel(SettingsService settings, PublicationService service,
        ILogger<PublicationListViewModel> logger, JobContextFactory factory, ClaimsPrincipal principal)
    {
        this.settings = settings;
        this.service = service;
        this.logger = logger;
        this.factory = factory;
        this.principal = principal;
    }

    public async Task LoadSettings(string settingsId)
    {
        try
        {
            // TODO: settings for SignalR topic for performing updates?
            await Task.Yield();
            // var channelSettings = await settings.LoadSettings<ChannelSettings>(channelName);
        }
        catch (FileNotFoundException)
        {
            // Just leave things as they are
        }
    }

    public IEnumerable<string> GetActiveFilters()
    {
        return new[] {
            (Name: nameof(IncludePostedMessages), Value: IncludePostedMessages),
            (Name: nameof(IncludeReceivedMessages), Value: IncludeReceivedMessages)
        }.Where(e => e.Value).Select(e => e.Name).ToArray();
    }

    private JobContext? cachedContext;
    public async void PublicationsQueryProvider(Action<IQueryable<TaskModels.Publication>> executor)
    {
        if (cachedContext is null)
        {
            try
            {
                using var context = await factory.CreateDbContext(principal);
                cachedContext = context;
                PublicationsQueryProvider(executor);
                return;
            }
            finally
            {
                cachedContext = null;
            }
        }
        else 
        {
            var bareQuery = BuildQuery(cachedContext);
            executor(bareQuery);
        }
    }

    private IQueryable<TaskModels.Publication> BuildQuery(IJobContext context)
    {
        var query = service.PublicationsQuery(context);
        if (ReceivedMessagesOnly) return query.WhereReceived();
        if (PostedMessagesOnly) return query.WherePosted();
        if (!PostedAndReceivedMessages) return query.Where(p => false);
        return query;
    }
}
