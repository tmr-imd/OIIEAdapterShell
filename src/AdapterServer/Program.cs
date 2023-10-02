using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AdapterServer.Extensions;
using AdapterServer.Pages.Publication;
using AdapterServer.Pages.Request;
using AdapterServer.Shared;
using AuthenticationExtesion.AWS;
using AuthenticationExtesion.Support;
using Microsoft.AspNetCore.Authentication;
using Notifications.UI;

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

var authExcludedEndpoints = new string[]
{
    "/api/notifications", // ISBM Notifications (whitelist source maybe?)
    "/healthz",           // Health Checks      (whitelist source maybe?)
    // "/_blazor",
    // "/pub-sub"
};

var authExcludedInternallyEndpoints = new string[]
{
    "/app/notifications-hub",
};

builder.Services.AddAuthentication(AwsLoadBalancerDefaults.AuthenticationScheme)
    .AddNoAuthenticationScheme(o => {})
    .AddAwsLoadBalancerAuthentication(o =>
    {
        o.ForwardDefaultSelector = ctx =>
        {
            Console.WriteLine("ForwardDefaultSelector: {0}", ctx.Request.Path);
            // ctx.Connection.ClientCertificate
            Console.WriteLine("Request coming into {0}", ctx.Connection.LocalIpAddress);
            Console.WriteLine("Request coming from {0}", ctx.Connection.RemoteIpAddress);
            if (ctx.Connection.LocalIpAddress is IPAddress local && IPAddress.IsLoopback(local)
                 && ctx.Connection.RemoteIpAddress is IPAddress remote && IPAddress.IsLoopback(remote)
                 && authExcludedInternallyEndpoints.Any(e => ctx.Request.Path.StartsWithSegments(e)))
            {
                Console.WriteLine("Local and remote IP are loopback address, using internal authentication.");
                return NoAuthenticationDefaults.AuthenticationScheme;
            }

            return authExcludedEndpoints.Any(e => ctx.Request.Path.StartsWithSegments(e)) ? NoAuthenticationDefaults.AuthenticationScheme : null;
        };
    });
builder.Services.AddAuthorization(o =>
{
    o.AddAdministratorsOnlyPolicy();
    o.AddNotificationsHubPolicy();
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
