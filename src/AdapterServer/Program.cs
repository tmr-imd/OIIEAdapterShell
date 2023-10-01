using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AdapterServer.Extensions;
using AdapterServer.Pages.Publication;
using AdapterServer.Pages.Request;
using AdapterServer.Shared;
using AuthenticationExtesion.AWS;
using AuthenticationExtesion.Support;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

builder.Services.AddAuthentication(AwsLoadBalancerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>("Notifications", o => {})
    .AddAwsLoadBalancerAuthentication(o =>
    {
        o.ForwardDefaultSelector = ctx =>
        {
            Console.WriteLine("ForwardDefaultSelector: {0}", ctx.Request.Path);
            return ctx.Request.Path.StartsWithSegments("/api/notifications") ? "Notifications" : null;
        };
    });

builder.Services.AddSingleton<INavigationConfiguration, NavigationConfiguration>();
builder.Services.AddSingleton<IScheduledJobsConfig<ManageRequestViewModel>, JobSchedulerForStructures>();
builder.Services.AddSingleton<IScheduledJobsConfig<ManagePublicationViewModel>, JobSchedulerForPubStructures>();

builder.Services.AddScoped<PublicationDetailViewModel, StructuresPublicationDetailViewModel>();
builder.Services.AddScoped<RequestResponseDetailViewModel, StructuresRequestResponseDetailViewModel>();

// Example SSL Certificate validation and logging
builder.Services.AddSingleton<ICertificateValidator, DefaultCertificateValidator>();
builder.Services.AddSingleton<RemoteCertificateValidationCallback>(ICertificateValidator.ValidationCallback);

// Example Health check
// (see https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-7.0)
builder.Services.AddHealthChecks();
    // .AddCheck<Checktype>("Check name");

builder.UseStartup<AdapterServer.Startup>();

var app = builder.Build<AdapterServer.Startup>();

// Add example health check middleware
app.MapHealthChecks("/healthz");

app.Run();
