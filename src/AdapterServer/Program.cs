using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AdapterServer.Extensions;
using AdapterServer.Pages.Publication;
using AdapterServer.Pages.Request;
using AdapterServer.Shared;

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

builder.Services.AddSingleton<INavigationConfiguration, NavigationConfiguration>();
builder.Services.AddSingleton<IScheduledJobsConfig<ManageRequestViewModel>, JobSchedulerForStructures>();
builder.Services.AddSingleton<IScheduledJobsConfig<ManagePublicationViewModel>, JobSchedulerForPubStructures>();

// Example SSL Certificate validation and logging
builder.Services.AddSingleton<ICertificateValidator, DefaultCertificateValidator>();
builder.Services.AddSingleton<RemoteCertificateValidationCallback>(ICertificateValidator.ValidationCallback);

builder.UseStartup<AdapterServer.Startup>();

var app = builder.Build<AdapterServer.Startup>();

app.Run();
