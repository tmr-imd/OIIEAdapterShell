using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;

using Notifications.Persistence;
using Notifications.Services;
using Notifications.Services.Internal;
using Microsoft.AspNetCore.Authorization;

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
        services.AddScoped<NotificationsJs>();
    }

    public static void AddNotifications(this IEndpointRouteBuilder app, string path)
    {
        app.MapHub<NotificationsHub>(path);
    }

    /// <summary>
    /// Adds an authorization policy for "NotificationsHubPolicy", optional delegate
    /// overrides the default policy settings.
    /// TODO: document the default policy.
    /// </summary>
    /// <param name="configurePolicy">Delegate to build the policy</param>
    public static void AddNotificationsHubPolicy(this AuthorizationOptions options,
            Action<AuthorizationPolicyBuilder>? configurePolicy = null)
    {
        options.AddPolicy(INotificationsHub.AUTHORIZATION_POLICY_NAME, policy =>
        {
            if (configurePolicy is null)
            {
                // TODO: sort out default policy
                // policy.RequireAssertion(ctx => true);
                policy.RequireUserName("Internal");
            }
            else
            {
                configurePolicy(policy);
            }
        });
    }
}