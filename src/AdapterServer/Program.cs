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

var builder = WebApplication.CreateBuilder(args);

// Concrete adapter implementations can add customisation by subclassing
// AdapterServer.Startup, or by customising directly in Program.cs

builder.UseStartup<AdapterServer.Startup>();

var app = builder.Build<AdapterServer.Startup>();

app.Run();
