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

var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection");
var hangfireStorage = builder.Configuration.GetSection("Hangfire").GetValue<string>( "Storage" );

// Add Hangfire services.
var hangfireConfig = builder.Services.HangfireConfiguration( hangfireConnection, hangfireStorage );
builder.Services.AddHangfire( hangfireConfig );

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddScoped( x => JobContextHelper.PrincipalFromString("AdapterServer") );
builder.Services.AddSingleton(new JobContextFactory(builder.Configuration));
builder.Services.AddSingleton(new CIRLibContextFactory(builder.Configuration));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var isbmSection = builder.Configuration.GetSection("Isbm");
builder.Services.Configure<ClientConfig>(isbmSection);
builder.Services.AddIsbmRestClient(isbmSection);

builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<StructureAssetService>();
builder.Services.AddScoped<RequestViewModel>();
builder.Services.AddScoped<ResponseViewModel>();
builder.Services.AddScoped<PublicationService>();
builder.Services.AddScoped<PublicationDetailViewModel>();
builder.Services.AddScoped<PublicationListViewModel>();
builder.Services.AddScoped<PublicationViewModel>();
builder.Services.AddScoped<ConfirmBODConfigViewModel>();
builder.Services.AddScoped<RegistryServices>();
builder.Services.AddScoped<CategoryServices>();
builder.Services.AddScoped<EntryServices>();
builder.Services.AddScoped<PropertyServices>();
builder.Services.AddScoped<PropertyValueServices>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseHangfireDashboard();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
