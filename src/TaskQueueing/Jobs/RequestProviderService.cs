using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;

namespace TaskQueueing.Jobs;

public class RequestProviderService
{
    public static async Task<IEnumerable<string>> GetSessionIds( IJobContext context )
    {
        return await context.ChannelSettings
            .Select(x => x.ProviderSessionId)
            .ToListAsync();
    }
}
