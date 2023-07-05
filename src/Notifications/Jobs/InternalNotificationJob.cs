using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;

using Notifications.Services.Internal;

namespace Notifications.Jobs;

public class InternalNotificationJob
{
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;
    private readonly ClaimsPrincipal _principal;

    public InternalNotificationJob(IHubContext<NotificationsHub, INotificationsClient> hubContext, ClaimsPrincipal principal)
    {
        _hubContext = hubContext;
        _principal = principal;
    }

    public async Task DoNotify()
    {
        await _hubContext.Clients.All.Notify("test", JsonSerializer.SerializeToDocument(new {Test = "example"}));
    }
}