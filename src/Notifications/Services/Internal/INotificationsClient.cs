using System.Text.Json;

namespace Notifications.Services.Internal;

public interface INotificationsClient
{
    public Task SyncConnectionId(string connectionId);

    public Task Notify(string topic, string notificationId);
}