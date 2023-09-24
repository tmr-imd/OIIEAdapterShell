
namespace Notifications.Services.Internal;

public interface INotificationsHub
{
    public Task SendMessage(string topic, string notificationId);
}