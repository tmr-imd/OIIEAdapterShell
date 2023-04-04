using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;

namespace TaskQueueing.Jobs;

public class RequestConsumerService
{
    public static async Task<IEnumerable<Request>> OpenRequests( IJobContext context )
    {
        return await context.Requests.Where(x => !x.Processed).ToListAsync();
    }
}
