using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Pages.Request;

public class PublicationService
{
    public async Task<IEnumerable<TaskModels.Publication>> ListPublications(IJobContext context)
    {
        return await context.Publications
            .OrderByDescending(x => x.DateCreated)
            .ToListAsync();
    }
}
