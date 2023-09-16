using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Services;

public class RequestService
{
    public static IQueryable<TaskModels.Request> RequestsQuery(IJobContext context)
    {
        return context.Requests
            .Include(x => x.Responses)
            .OrderByDescending(x => x.DateCreated);
    }

    public static async Task<IEnumerable<TaskModels.Request>> ListRequests(IJobContext context)
    {
        var requests = await RequestsQuery(context)
            .ToListAsync();

        return requests
            .GroupBy(x => x.RequestId)
            .OrderByDescending(x => x.Max( y => y.DateCreated ))
            .SelectMany(x => x);
    }

    public static async Task<TaskModels.Request?> GetRequest(IJobContext context, Guid requestId)
    {
        return await context.Requests
            .Where(x => x.Id == requestId)
            .Include(x => x.Responses)
            .FirstOrDefaultAsync();
    }
}
