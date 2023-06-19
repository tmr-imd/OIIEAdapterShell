using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Data;
using AdapterServer.Pages;
using AdapterServer.Pages.Request;
using AdapterServer.Pages.Publication;
using Hangfire;
using TaskQueueing.Persistence;
using TaskQueueing.Jobs;
using CIRLib.Extensions;
using Oiie.Settings;
using System.Text;
using System.Text.Json;
using TaskQueueing.ObjectModel.Models;

namespace AdapterServer;

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
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

            // In dev plain http helps us avoid certificate issues with the 
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseHangfireDashboard();

        routes.MapBlazorHub();
        routes.MapFallbackToPage("/_Host");

        routes.MapPut("/api/notifications/{sessionId}/{messageId}", async (string sessionId, string messageId, HttpRequest request, ILogger<Startup> log) =>
        {
            try
            {
                log.LogInformation($"sessionId: {sessionId}, messageId: {messageId} - Notification received");

                using StreamReader reader = new(request.Body, Encoding.UTF8, true, 1024, true);
                var content = await reader.ReadToEndAsync();

                log.LogInformation($"sessionId: {sessionId}, messageId: {messageId} - {content}");

                var notifyBody = JsonSerializer.Deserialize<NotifyBody>(content);

                if (notifyBody is not null)
                {
                    var jobId = BackgroundJob.Enqueue<NotificationJob>(x => x.Notify(sessionId, messageId, notifyBody));

                    log.LogInformation($"sessionId: {sessionId}, messageId: {messageId} - NotificationJob {jobId} enqueued");
                }

            }
            catch (Exception ex)
            {
                log.LogError(ex, $"sessionId: {sessionId}, messageId: {messageId} - Error processing notification");
            }

            return Results.NoContent();
        });
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        var hangfireConnection = Configuration.GetConnectionString("HangfireConnection");
        var hangfireStorage = Configuration.GetSection("Hangfire").GetValue<string>( "Storage" );

        // Add Hangfire services.
        var hangfireConfig = services.HangfireConfiguration(hangfireConnection, hangfireStorage);
        services.AddHangfire(hangfireConfig);

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        services.AddScoped(x => JobContextHelper.PrincipalFromString("AdapterServer"));
        services.AddSingleton(new JobContextFactory(Configuration));

        // Add services to the container.
        services.AddRazorPages();
        services.AddServerSideBlazor();

        var isbmSection = Configuration.GetSection("Isbm");
        services.Configure<ClientConfig>(isbmSection);
        services.AddIsbmRestClient(isbmSection);

        //CIR Config
        services.AddCIRServices(Configuration);

        services.AddScoped<SettingsService>();
        services.AddScoped<StructureAssetService>();
        services.AddScoped<RequestViewModel>();
        services.AddScoped<ManageRequestViewModel>();
        services.AddScoped<ResponseViewModel>();
        services.AddScoped<ManagePublicationViewModel>();
        services.AddScoped<PublicationDetailViewModel>();
        services.AddScoped<PublicationListViewModel>();
        services.AddScoped<PublicationViewModel>();
        services.AddScoped<PublicationService>();
        services.AddScoped<ConfirmBODConfigViewModel>();
    }
}