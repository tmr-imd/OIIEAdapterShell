using System.Text.Json;
using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;
using Notifications.Persistence;

namespace Notifications.Services;

/// <summary>
/// Default concrete implementation of the INotificationService interface.
/// </summary>
/// <remarks>
/// Uses SignalR for local/internal notifications and email for external notifications.
/// Uses Hangifire jobs to perform notifications asynchronously across replicants.
/// </remarks>
public class NotificationService : INotificationService
{
    public static NotificationsContextFactory ContextFactory { get; set; } = null!;

    public async void Notify<T>(Scope scope, string topic, T data, string origin)
    {
        using var context = await ContextFactory.CreateDbContext(origin);

        var notification = new Notification()
        {
            Id = Guid.NewGuid(),
            Scope = scope,
            Topic = topic,
            ReadState = ReadState.Undefined,
            Data = JsonSerializer.SerializeToDocument(data, JsonSerializerOptions.Default)
        };
        context.Add(notification);
        context.SaveChanges();

        // TODO: Do specfic processing based on scope
    }

    public string RegisterLocal(string topic, Action<object> callback, params string[] todo)
    {
        throw new NotImplementedException();
    }

    public void Unregister(string id)
    {
        throw new NotImplementedException();
    }
}