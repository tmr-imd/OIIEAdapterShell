
namespace Notifications.Services.Internal;

public interface INotificationsHub
{
    public const string AUTHORIZATION_POLICY_NAME = "NotificationsHubPolicy";

    public Task SendMessage(string topic, string notificationId);
}