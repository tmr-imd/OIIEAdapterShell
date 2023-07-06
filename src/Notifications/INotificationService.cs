using System.Text.Json;
using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;

namespace Notifications;

public interface INotificationService
{
    public Task Notify<T>(Scope scope, string topic, T data, string origin);

    public Task<string> RegisterLocal(string topic, Action<Notification> callback, params string[] todo);

    public Task Unregister(string id);
}