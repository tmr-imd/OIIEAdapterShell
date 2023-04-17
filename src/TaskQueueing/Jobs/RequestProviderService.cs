using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;

namespace TaskQueueing.Jobs;

public class RequestProviderService
{
    public static async Task<IEnumerable<Request>> OpenRequests(IJobContext context)
    {
        return await context.Requests.WhereReceived().WhereUnprocessed().ToListAsync();
    }

    public static async Task<Request?> GetOpenRequest(string requestId, IJobContext context)
    {
        return await context.Requests.Where(x => x.RequestId == requestId)
            .WhereReceived()
            .WhereUnprocessed()
            .FirstOrDefaultAsync();
    }
}
