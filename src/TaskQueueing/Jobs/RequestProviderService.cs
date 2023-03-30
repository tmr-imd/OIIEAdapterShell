using Hangfire;
using Hangfire.Server;
using Isbm2Client.Interface;
using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.Persistence;

namespace TaskQueueing.Jobs;

public class RequestProviderService
{
    public async Task<IEnumerable<string>> GetRequestSessions( IJobContext context )
    {
        return await context.ChannelSettings.Select( x => x.Name ).ToListAsync();
    }
}
