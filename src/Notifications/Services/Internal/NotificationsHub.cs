using Microsoft.AspNetCore.SignalR;
using Notifications.ObjectModel.Models;

using System.Text.Json;

namespace Notifications.Services.Internal;

public class NotificationsHub : Hub<INotificationsClient>
{
    public override Task OnConnectedAsync()
    {
        // TODO: switch to logging, not just Console.Out
        Console.WriteLine($"NotificationsHub connection opened: {Context.ConnectionId}");
        Clients.Client(Context.ConnectionId).SyncConnectionId(Context.ConnectionId);
        // Groups.AddToGroupAsync(Context.ConnectionId, "TEMP");
        return base.OnConnectedAsync();
    }

    // public async Task SendMessage(Notification notification)
    // {
    //     Console.WriteLine($"Notifying all for {notification}");
    //     await Clients.All.Notify(notification.Topic, notification.Data);
    // }

    public async Task SendMessage(string topic, string content)
    {
        Console.WriteLine($"Notifying all for {topic}: {content}");
        await Clients.All.Notify(topic, JsonSerializer.SerializeToDocument(content));
    }
}