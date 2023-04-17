using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel;
using TaskQueueing.ObjectModel.Models;

namespace TaskQueueing.Jobs;

public static class IQueryableExtensions
{
    public static IQueryable<T> WherePosted<T>(this IQueryable<T> query) where T : AbstractMessage
    {
        return query.Where(x => (x.State & MessageState.Posted) == MessageState.Posted);
    }

    public static IQueryable<T> WhereReceived<T>(this IQueryable<T> query) where T : AbstractMessage
    {
        return query.Where(x => (x.State & MessageState.Received) == MessageState.Received);
    }

    public static IQueryable<T> WhereUnprocessed<T>(this IQueryable<T> query) where T : AbstractMessage
    {
        return query.Where(x => (x.State & MessageState.Processed) != MessageState.Processed);
    }
}
