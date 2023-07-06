using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;

using Notifications.Services.Internal;

namespace Notifications.Jobs;

public class InternalNotificationJob : INotificationsHub
{
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;
    private readonly ClaimsPrincipal _principal;

    public InternalNotificationJob(IHubContext<NotificationsHub, INotificationsClient> hubContext, ClaimsPrincipal principal)
    {
        _hubContext = hubContext;
        _principal = principal;
    }

    public async Task SendMessage(string topic, string notificationId, Hangfire.Server.PerformContext? ctxt)
    {
        Console.WriteLine($"Notifying all for {topic}: {notificationId}");
        await _hubContext.Clients.All.Notify(topic, notificationId);
    }

    public async Task SendMessage(string topic, string notificationId)
    {
        await SendMessage(topic, notificationId, null);
    }
}