using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;
using System.Text.Json;
using Hangfire;
using Notifications.ObjectModel;
using Notifications.ObjectModel.Models;
using Notifications.Persistence;
using Notifications.Jobs;

namespace Notifications.Services;

using Internal;

/// <summary>
/// Default concrete implementation of the INotificationService interface.
/// </summary>
/// <remarks>
/// Uses SignalR for local/internal notifications and email for external notifications.
/// Uses Hangifire jobs to perform notifications asynchronously across replicants.
/// </remarks>
public class NotificationService : INotificationService
{
    public static NotificationsContextFactory ContextFactory { get; set; } = null!;

    private static readonly Dictionary<string, HubConnection> _hubConnections = new();

    private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;
    private readonly ClaimsPrincipal _principal;
    private readonly Uri _hubUrl;

    public NotificationService(IHubContext<NotificationsHub, INotificationsClient> hubContext, ClaimsPrincipal principal, IServer server)
    {
        _hubContext = hubContext;
        _principal = principal;
        // Refer to https://swimburger.net/blog/dotnet/how-to-get-aspdotnet-core-server-urls for different
        // mechanisms for accessing the URL addresses of the application. We currently use the NavigationManger
        // as for the front-end, but we may actually be able to just use the internal addresses.
        // *Is there a safe default or should we throw an exception?
        var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault() ?? "http://localhost";
        var uriBuilder = new UriBuilder(address)
        {
            Path = "/app/notifications-hub"
        };
        _hubUrl = uriBuilder.Uri;
    }

    public async Task Notify<T>(Scope scope, string topic, T data, string origin, ReadState readState = ReadState.Unread)
    {
        if (readState == ReadState.Undefined) throw new Exception("Must provide a non-zero (Undefined) ReadState for the notification.");

        using var context = await ContextFactory.CreateDbContext(origin);

        var notification = new Notification()
        {
            Id = Guid.NewGuid(),
            Scope = scope,
            Topic = topic,
            ReadState = readState,
            Data = JsonSerializer.SerializeToDocument(data, JsonSerializerOptions.Default)
        };
        context.Add(notification);
        context.SaveChanges();

        switch (scope)
        {
            case Scope.Local:
                await new InternalNotificationJob(_hubContext, _principal).SendMessage(topic, notification.Id.ToString());
                break;
            case Scope.Internal:
                // XXX: need to apply to each replicant's queue
                BackgroundJob.Enqueue<InternalNotificationJob>(x => x.SendMessage(topic, notification.Id.ToString(), null!));
                break;
            case Scope.External:
                BackgroundJob.Enqueue<EmailNotificationJob>(x => x.SendMessage(topic, notification.Id, null!));
                break;
            case Scope.InternalAndExternal:
                BackgroundJob.Enqueue<EmailNotificationJob>(x => x.SendMessage(topic, notification.Id, null!));
                // XXX: need to apply to each replicant's queue
                BackgroundJob.Enqueue<InternalNotificationJob>(x => x.SendMessage(topic, notification.ToString(), null!));
                break;
            case Scope.Undefined:
            // Fall through to default
            default:
                break;
        }
    }

    public async Task NotifyUsers<T>(Scope scope, string topic, T data, string origin)
    {
        await Notify<T>(scope, topic, data, origin, ReadState.UserDependent);
    }

    public async Task NotifyApp<T>(Scope scope, string topic, T data, string origin)
    {
        await Notify<T>(scope, topic, data, origin, ReadState.AppDependent);
    }

    public async Task<string> RegisterLocal(string topic, Action<Notification> callback, params string[] todo)
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<string>("SyncConnectionId", (connectionId) => {
            // Not sure I actually need this, but possibly want to register specific topics for groupings.
            if (hubConnection.ConnectionId != connectionId)
            {
                Console.WriteLine($"Conn IDs do not match for some reason {hubConnection.ConnectionId} != {connectionId}");
            }
        });

        hubConnection.On<string, string>("Notify", async (t, notificationId) =>
        {
            // XXX: does not yet check user read states (does it need to here?)
            if (t == topic)
            {
                using var context = await ContextFactory.CreateDbContext("internal");
                var notification = context.Notifications
                    .Where(n => n.Id == Guid.Parse(notificationId) && n.ReadState != ReadState.Read)
                    .FirstOrDefault();
                if (notification is null) return;
                await Task.Run(() => callback(notification));
            }
        });

        await hubConnection.StartAsync();

        if (hubConnection.ConnectionId is null)
        {
            await hubConnection.DisposeAsync();
            throw new Exception("Failed to connect to SignalR hub for local notifications");
        }

        _hubConnections[hubConnection.ConnectionId] = hubConnection;

        return hubConnection.ConnectionId;
    }

    public async Task Unregister(string id)
    {
        if (!_hubConnections.ContainsKey(id)) return;
        Console.WriteLine("NotificationsHub");

        var hubConnection = _hubConnections[id];
        _hubConnections.Remove(id);
        await hubConnection.DisposeAsync();
    }
}