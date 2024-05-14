using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskModels = TaskQueueing.ObjectModel.Models;

namespace AdapterServer.Services;

public class PublicationService
{
    public IQueryable<TaskModels.Publication> PublicationsQuery(IJobContext context)
    {
        return context.Publications
            .OrderByDescending(x => x.DateCreated);
    }

    public async Task<IEnumerable<TaskModels.Publication>> ListPublications(IJobContext context)
    {
        return await PublicationsQuery(context)
            .ToListAsync();
    }

    public static async Task<TaskModels.Publication?> GetPublication(IJobContext context, Guid publicationId)
    {
        return await context.Publications
            .Where(x => x.Id == publicationId)
            .FirstOrDefaultAsync();
    }

    public static async Task<string?> JobIdFromSession(string sessionId, TaskQueueing.Persistence.JobContext context)
    {
        return await context.Sessions
            .Where(x => x.SessionId == sessionId)
            .Select(x => x.RecurringJobId)
            .FirstOrDefaultAsync();
    }
}
