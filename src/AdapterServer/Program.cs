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
builder.Services.AddSingleton<IScheduledJobsConfig<ManageRequestViewModel<RequestViewModel.MessageTypes>>, JobSchedulerForStructures>();
builder.Services.AddSingleton<IScheduledJobsConfig<ManagePublicationViewModel<PublicationViewModel.MessageTypes>>, JobSchedulerForPubStructures>();

builder.Services.AddScoped<ManageRequestViewModel<RequestViewModel.MessageTypes>>();
builder.Services.AddScoped<ManagePublicationViewModel<PublicationViewModel.MessageTypes>>();
// builder.Services.AddScoped<PublicationViewModel>(); // TODO: to be moved here from Startup
builder.Services.AddScoped<PublicationDetailViewModel, StructuresPublicationDetailViewModel>();
builder.Services.AddScoped<RequestResponseDetailViewModel, StructuresRequestResponseDetailViewModel>();

// Example SSL Certificate validation and logging
builder.Services.AddSingleton<ICertificateValidator, DefaultCertificateValidator>();
builder.Services.AddSingleton<RemoteCertificateValidationCallback>(ICertificateValidator.ValidationCallback);

builder.UseStartup<AdapterServer.Startup>();

var app = builder.Build<AdapterServer.Startup>();

app.Run();
