using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Components.Publications;
using AdapterServer.Components.RequestResponse;
using AdapterServer.Data;
using AdapterServer.Pages;
using AdapterServer.Pages.Request;
using AdapterServer.Pages.Publication;
using AdapterServer.Services;
using Hangfire;
using TaskQueueing.Persistence;
using TaskQueueing.Jobs;
using CIRLib.Extensions;
using Oiie.Settings;
using System.Text;
using System.Text.Json;
using TaskQueueing.ObjectModel.Models;
using OiieAdminUi.Authorization;
using System.Net.Security;
using AdapterServer.Shared;
using Notifications.UI;
using AuthenticationExtesion.AWS;
using AuthenticationExtesion.Extensions;

namespace AdapterServer;

/// <summary>
/// Old-style Startup class to allow capture of common configuration for
/// OIIE Adapters. OIIE Adapters for specific back-end systems can provide
/// dedicated configuration in Program(.cs) and call the Startup methods.
/// </summary>
public class Startup
{
    protected IConfigurationRoot Configuration { get; }

    public Startup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }

    public virtual void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseForwardedHeaders(); // must be before UseHsts
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

            // In dev plain http helps us avoid certificate issues with the 
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseForwardedHeaders();
        }

        app.UseStaticFiles();

        app.UseRouting();

        if (env.IsDevelopment())
        {
            // Add a middleware that injects AWS Load Balancer style HTTP headers for Authorization
            app.UseInjectLoadBalancerHeaders();
        }

        // Authentication for all, but exclude notification route?
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHangfireDashboard(options: new DashboardOptions()
        {
            Authorization = new[] { new HangfireDashboardAuthFilter() }
        });

        // Ensure the certificate validator is instantiated, if provided.
        var certValidator = app.ApplicationServices.GetService<ICertificateValidator>();
        if (certValidator is not null && ICertificateValidator.Instance is null)
        {
            ICertificateValidator.Instance = certValidator;
        }

        app.UseUserServiceMiddleware();

        routes.AddNotifications("/app/notifications-hub");
        routes.MapFallbackToPage("/_Host");

        routes.MapPut("/api/notifications/{sessionId}/{messageId}", async (string sessionId, string messageId, HttpRequest request, ILogger<Startup> log) =>
        {
            try
            {
                // Health check
                if (EmptyId(sessionId) && EmptyId(messageId))
                {
                    log.LogInformation("sessionId: {sessionId}, messageId: {messageId} - Health Check", sessionId, messageId);

                    return Results.Ok();
                }

                log.LogInformation("sessionId: {sessionId}, messageId: {messageId} - Notification received", sessionId, messageId);

                using StreamReader reader = new(request.Body, Encoding.UTF8, true, 1024, true);
                var content = await reader.ReadToEndAsync();

                log.LogInformation("sessionId: {sessionId}, messageId: {messageId} - {content}", sessionId, messageId, content);

                var notifyBody = !string.IsNullOrEmpty(content) ? JsonSerializer.Deserialize<NotifyBody>(content) : new NotifyBody();

                if (notifyBody is not null)
                {
                    var jobId = BackgroundJob.Enqueue<NotificationJob>(x => x.Notify(sessionId, messageId, notifyBody));

                    log.LogInformation("sessionId: {sessionId}, messageId: {messageId} - NotificationJob {jobId} enqueued", sessionId, messageId, jobId);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "sessionId: {sessionId}, messageId: {messageId} - Error processing notification", sessionId, messageId);
            }

            return Results.NoContent();
        });
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        var hangfireConnection = Configuration.GetConnectionString("HangfireConnection");
        var hangfireStorage = Configuration.GetSection("Hangfire").GetValue<string>("Storage");
        var hangfireQueues = Configuration.GetSection("Hangfire:Queues").Get<string[]>();

        // Add Hangfire services.
        var hangfireConfig = services.HangfireConfiguration(hangfireConnection, hangfireStorage);
        services.AddHangfire(hangfireConfig);

        // Add the processing server as IHostedService
        services.AddHangfireServer(opts => opts.Queues = hangfireQueues);

        services.AddUserServices();

        // TODO: remove this artificially introduced claims principal (utilise the UserService instead)
        services.AddScoped(x => JobContextHelper.PrincipalFromString("AdapterServer"));
        services.AddSingleton(new JobContextFactory(Configuration));

        // Add services to the container.
        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.Configure<ForwardedHeadersOptions>(opts =>
        {
            // Configure header fowarding for load balancers/proxies
        });

        // For testing authentication in development mode.
        var injectSection = Configuration.GetSection(InjectLoadBalancerHeadersMiddleware.InjectLoadBalancerHeadersConfigKey);
        if (injectSection.GetChildren().Any()) services.Configure<InjectLoadBalancerHeadersOptions>(injectSection);

        var isbmSection = Configuration.GetSection("Isbm");
        services.Configure<ClientConfig>(isbmSection);
        var certificateValidationCallback = services
            .Where(sd => sd.ServiceType == typeof(RemoteCertificateValidationCallback) && sd.ImplementationInstance is not null)
            .Select(sd => sd.ImplementationInstance)
            .Cast<RemoteCertificateValidationCallback?>()
            .FirstOrDefault();
        services.AddIsbmRestClient(isbmSection, certificateValidationCallback);

        // CIR Config
        services.AddCIRServices(Configuration);

        services.AddScoped<SettingsService>();
        services.AddScoped<StructureAssetService>();
        services.AddScoped<RequestViewModel>();
        services.AddScoped<RequestsListViewModel>();
        services.AddScoped<RequestService>();
        services.AddScoped<ManageRequestViewModel>();
        services.AddScoped<ManagePublicationViewModel>();
        services.AddScoped<PublicationListViewModel>();
        services.AddScoped<PublicationViewModel>();
        services.AddScoped<PublicationService>();
        services.AddScoped<ConfirmBODConfigViewModel>();

        services.AddNotifications(Configuration);

        services.AddRoleMappingServices();
    }

    private static bool EmptyId(string id)
    {
        if (Guid.TryParse(id, out Guid idGuid))
            return idGuid == Guid.Empty;

        if (int.TryParse(id, out int idInt))
            return idInt == 0;

        return string.IsNullOrEmpty(id);
    }
}