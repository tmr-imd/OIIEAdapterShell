using System.Security.Claims;

using Notifications.Services.Internal;

namespace Notifications.Jobs;

public class EmailNotificationJob
{
    private readonly ClaimsPrincipal _principal;

    public EmailNotificationJob(ClaimsPrincipal principal)
    {
        _principal = principal;
    }

    public async Task SendMessage(string topic, Guid notificationId, Hangfire.Server.PerformContext? ctxt)
    {
        Console.WriteLine($"Notifying email for {topic} (from {_principal.Identity?.Name}): {notificationId}");
        await Task.Run(() => throw new NotImplementedException("Once user logins are implemented we will complete this."));
    }
}
