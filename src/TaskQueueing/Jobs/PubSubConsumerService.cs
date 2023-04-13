using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;

namespace TaskQueueing.Jobs;

public class PubSubConsumerService
{
    public static async Task<IEnumerable<Publication>> UnprocessedPublications(IJobContext context)
    {
        return await context.Publications.Where(x => !x.Processed).ToListAsync();
    }

    public static async Task<Publication?> GetUnprocessedPublication(string messageId, IJobContext context)
    {
        return await context.Publications.Where(x => x.MessageId == messageId && !x.Processed).FirstOrDefaultAsync();
    }
}
