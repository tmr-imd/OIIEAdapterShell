using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;

namespace AdapterServer.Pages;

public class RequestService
{
    public async Task<IEnumerable<TaskQueueing.ObjectModel.Models.Request>> ListRequests( IJobContext context )
    {
        return await context.Requests
            .OrderByDescending( x => x.DateCreated )
            .ToListAsync();
    }
}
