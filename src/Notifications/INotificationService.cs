using Notifications.ObjectModel;

namespace Notifications;

public interface INotificationService
{
    public void Notify<T>(Scope scope, string topic, T data, string origin);

    public string RegisterLocal(string topic, Action<object> callback, params string[] todo);

    public void Unregister(string id);
}