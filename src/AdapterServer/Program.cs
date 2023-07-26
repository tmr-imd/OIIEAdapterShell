using AdapterServer.Extensions;
using AdapterServer.Pages.Request;
using AdapterServer.Shared;

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

NavigationConfiguration.selectedComponent = (typeof(AdapterServer.Shared.NavMenu));
builder.Services.AddSingleton<INavigationConfiguration, NavigationConfiguration>();
builder.Services.AddSingleton<IScheduledJobsConfig, JobSchedulerForStructures>();

builder.UseStartup<AdapterServer.Startup>();

var app = builder.Build<AdapterServer.Startup>();

app.Run();
