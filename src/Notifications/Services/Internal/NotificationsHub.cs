using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Notifications.ObjectModel.Models;

using System.Text.Json;

namespace Notifications.Services.Internal;

[Authorize(INotificationsHub.AUTHORIZATION_POLICY_NAME)]
public class NotificationsHub : Hub<INotificationsClient>, INotificationsHub
{
    public override Task OnConnectedAsync()
    {
        // TODO: switch to logging, not just Console.Out
        Console.WriteLine($"NotificationsHub connection opened: {Context.ConnectionId}");
        Clients.Client(Context.ConnectionId).SyncConnectionId(Context.ConnectionId);
        // Groups.AddToGroupAsync(Context.ConnectionId, "TEMP");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"NotificationsHub connection closed: {Context.ConnectionId} for reason {exception?.Message}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string topic, string notificationId)
    {
        Console.WriteLine($"Notifying all for {topic}: {notificationId}");
        await Clients.All.Notify(topic, notificationId);
    }
}