using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;

using Notifications.Persistence;
using Notifications.Services;
using Notifications.Services.Internal;

namespace Notifications.UI;

public static class NotificationsExtensions
{
    public static void AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });

        var contextFactory = new NotificationsContextFactory(configuration);
        NotificationService.ContextFactory = contextFactory;
        services.AddSingleton(contextFactory);
        services.AddScoped<INotificationService, NotificationService>();
    }

    public static void AddNotifications(this IEndpointRouteBuilder app, string path)
    {
        app.MapHub<NotificationsHub>(path);
    }
}