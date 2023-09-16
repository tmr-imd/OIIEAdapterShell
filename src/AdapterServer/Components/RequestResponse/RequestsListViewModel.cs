using System.Security.Claims;
using AdapterServer.Services;
using Microsoft.EntityFrameworkCore;
using Oiie.Settings;
using TaskQueueing.Jobs;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;
using TaskQueueing.Persistence;

namespace AdapterServer.Components.RequestResponse;

public class RequestsListViewModel
{
    public bool IncludeReceivedMessages { get; set; } = true;
    public bool IncludePostedMessages { get; set; } = true;
    public bool ErrorsOnly { get; set; } = false;

    public bool ReceivedMessagesOnly => IncludeReceivedMessages && !IncludePostedMessages;
    public bool PostedMessagesOnly => !IncludeReceivedMessages && IncludePostedMessages;
    public bool PostedAndReceivedMessages => IncludeReceivedMessages && IncludePostedMessages;

    private SettingsService settings;
    private RequestService service;
    private ILogger<RequestsListViewModel> logger;
    private JobContextFactory factory;
    private ClaimsPrincipal principal;

    public RequestsListViewModel(SettingsService settings, RequestService service,
        ILogger<RequestsListViewModel> logger, JobContextFactory factory, ClaimsPrincipal principal)
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
        return new (string Name, bool Value)[] {
            (Name: nameof(IncludePostedMessages), Value: IncludePostedMessages),
            (Name: nameof(IncludeReceivedMessages), Value: IncludeReceivedMessages)
        }.Where(e => e.Value).Select(e => e.Name).ToArray();
    }

    private IJobContext? cachedContext;
    public async void RequestsQueryProvider(Action<IQueryable<Request>> executor)
    {
        if (cachedContext is null)
        {
            try
            {
                using var context = await factory.CreateDbContext(principal);
                cachedContext = context;
                RequestsQueryProvider(executor);
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

    private IQueryable<Request> BuildQuery(IJobContext context)
    {
        var query = RequestService.RequestsQuery(context)
            .AsNoTrackingWithIdentityResolution();

        if (ErrorsOnly) query = query.WhereRequestOrResponseFailed();

        if (ReceivedMessagesOnly) return query.WhereReceived();
        if (PostedMessagesOnly) return query.WherePosted();
        if (!PostedAndReceivedMessages) return query.Where(p => false);
        return query;
    }
}