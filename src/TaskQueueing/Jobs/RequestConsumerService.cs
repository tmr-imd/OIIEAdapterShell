using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;

namespace TaskQueueing.Jobs;

public class RequestConsumerService
{
    public static async Task<IEnumerable<Request>> OpenRequests(IJobContext context)
    {
        return await context.Requests.WherePosted().WhereUnprocessed().Include(x => x.Responses).ToListAsync();
    }

    public static async Task<Request?> GetOpenRequest(string requestId, IJobContext context)
    {
        return await context.Requests.Where(x => x.RequestId == requestId)
            .WherePosted()
            .WhereUnprocessed()
            .Include(x => x.Responses)
            .FirstOrDefaultAsync();
    }

    public static async Task<Response?> GetOpenResponse(string requestId, string responseId, IJobContext context)
    {
        return await context.Responses.Where(x => x.RequestId == requestId && x.ResponseId == responseId)
            .WhereReceived()
            .WhereUnprocessed()
            .Include(x => x.Request)
            .FirstOrDefaultAsync();
    }
}
