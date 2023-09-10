using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;

namespace Notifications.Persistence;

public static class IQueryableExtensions
{
    public static IQueryable<T> WhereUnread<T>(this IQueryable<T> query) where T : Notification
    {
        return query.Where(x => (x.ReadState & ReadState.Read) == ReadState.Undefined);
    }
    // public static IQueryable<T> WherePosted<T>(this IQueryable<T> query) where T : AbstractMessage
    // {
    //     return query.Where(x => (x.State & MessageState.Posted) == MessageState.Posted);
    // }

    // public static IQueryable<T> WhereReceived<T>(this IQueryable<T> query) where T : AbstractMessage
    // {
    //     return query.Where(x => (x.State & MessageState.Received) == MessageState.Received);
    // }

    // public static IQueryable<T> WhereUnprocessed<T>(this IQueryable<T> query) where T : AbstractMessage
    // {
    //     return query.Where(x => (x.State & MessageState.Processed) == MessageState.Undefined);
    // }
}
