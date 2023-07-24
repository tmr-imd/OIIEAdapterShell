using AdapterServer.Extensions;
using AdapterServer.Pages.Request;

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

builder.UseStartup<AdapterServer.Startup>();
builder.Services.AddSingleton<IScheduledJobsConfig, JobSchedulerForStructures>();

var app = builder.Build<AdapterServer.Startup>();

app.Run();
