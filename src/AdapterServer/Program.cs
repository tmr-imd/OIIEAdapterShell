using Isbm2Client.Model;
using AdapterServer.Extensions;
using AdapterServer.Pages;
using AdapterServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var clientConfig = builder.Configuration.GetSection("Isbm").Get<ClientConfig>();
builder.Services.AddIsbmRestClient(clientConfig);

builder.Services.AddScoped<StructureAssetService>();
builder.Services.AddScoped<RequestViewModel>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
