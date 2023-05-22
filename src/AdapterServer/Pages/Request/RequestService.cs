using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Request;

public class RequestService
{
    public static async Task<IEnumerable<TaskModels.Request>> ListRequests(IJobContext context)
    {
        var requests = await context.Requests
            .OrderBy(x => x.DateCreated)
            .Include(x => x.Responses)
            .ToListAsync();

        return requests
            .GroupBy( x => x.RequestId )
            .OrderByDescending( x => x.Max( y => y.DateCreated ) )
            .SelectMany( x => x );
    }

    public static async Task<TaskModels.Request?> GetRequest(IJobContext context, Guid requestId)
    {
        return await context.Requests
            .Where(x => x.Id == requestId)
            .Include(x => x.Responses)
            .FirstOrDefaultAsync();
    }
}
