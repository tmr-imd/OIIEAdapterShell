using Microsoft.EntityFrameworkCore;
using TaskQueueing.ObjectModel.Enums;
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
        return query.Where(x => (x.State & MessageState.Processed) == MessageState.Undefined);
    }

    public static IQueryable<T> WhereFailed<T>(this IQueryable<T> query) where T : AbstractMessage
    {
        return query.Where(x => (x.State & MessageState.Error) == MessageState.Error);
    }

    public static IQueryable<T> WhereRequestOrResponseFailed<T>(this IQueryable<T> query) where T : Request
    {
        return query.Include(x => x.Responses)
            .Where(x => (x.State & MessageState.Error) == MessageState.Error || x.Responses.Any(y => (y.State & MessageState.Error) == MessageState.Error));
    }
}
