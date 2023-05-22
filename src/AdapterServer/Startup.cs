using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Data;
using AdapterServer.Pages;
using AdapterServer.Pages.Request;
using AdapterServer.Pages.Publication;
using Hangfire;
using TaskQueueing.Persistence;
using TaskQueueing.Data;
using CIRLib.Persistence;
using CIRLib.UI.Services;

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
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseHangfireDashboard();

        routes.MapBlazorHub();
        routes.MapFallbackToPage("/_Host");
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
        services.AddSingleton(new CIRLibContextFactory(Configuration));

        // Add services to the container.
        services.AddRazorPages();
        services.AddServerSideBlazor();

        var isbmSection = Configuration.GetSection("Isbm");
        services.Configure<ClientConfig>(isbmSection);
        services.AddIsbmRestClient(isbmSection);

        services.AddScoped<SettingsService>();
        services.AddScoped<StructureAssetService>();
        services.AddScoped<RequestViewModel>();
        services.AddScoped<ResponseViewModel>();
        services.AddScoped<PublicationService>();
        services.AddScoped<PublicationDetailViewModel>();
        services.AddScoped<PublicationListViewModel>();
        services.AddScoped<PublicationViewModel>();
        services.AddScoped<ConfirmBODConfigViewModel>();
        services.AddScoped<RegistryServices>();
        services.AddScoped<CategoryServices>();
        services.AddScoped<EntryServices>();
        services.AddScoped<PropertyServices>();
        services.AddScoped<PropertyValueServices>();
    }
}