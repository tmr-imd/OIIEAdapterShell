using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Data;
using AdapterServer.Pages;
using AdapterServer.Pages.Request;
using AdapterServer.Pages.Publication;
using AdapterServer.Shared;
using Hangfire;
using TaskQueueing.Persistence;
using TaskQueueing.Jobs;
using CIRLib.Persistence;
using CIRServices;
using CIRLib.Extensions;
using Oiie.Settings;

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

        routes.MapPut("/api/notifications/{sessionId}/{messageId}", (string sessionId, string messageId) =>
        {
            // Queue job (complete with DI)...
            BackgroundJob.Enqueue<NotificationJob>(x => x.Notify(sessionId, messageId));

            // ...and return immediately!
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
        services.AddScoped<PublicationService>();
        services.AddScoped<PublicationDetailViewModel>();
        services.AddScoped<PublicationListViewModel>();
        services.AddScoped<PublicationViewModel>();
        services.AddScoped<ConfirmBODConfigViewModel>();

        NavigationConfiguration.selectedComponent = (typeof(AdapterServer.Shared.NavMenu));
        services.AddSingleton<INavigationConfiguration, NavigationConfiguration>();
    }
}