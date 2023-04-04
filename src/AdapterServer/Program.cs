using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Pages;
using AdapterServer.Data;
using Hangfire;
using Hangfire.SqlServer;
using TaskQueueing.Persistence;
using TaskQueueing.Data;
using AdapterServer.Pages.Request;
using AdapterServer.Pages.Publication;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddScoped( x => JobContextHelper.PrincipalFromString("AdapterServer") );
builder.Services.AddSingleton(new JobContextFactory(builder.Configuration));

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
builder.Services.AddScoped<PublicationViewModel>();

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
