
namespace AdapterServer.Extensions;

public static class StartupExtensions
{
    private static Startup StartupInstance = null!;

    public static T UseStartup<T>(this WebApplicationBuilder builder) where T : Startup
    {
        T startup = Activator.CreateInstance(typeof(T), builder.Configuration, builder.Environment) as T ?? throw new Exception($"Unable to instantiate type {typeof(T).FullName}");
        StartupInstance = startup;

        startup.ConfigureServices(builder.Services);

        return startup;
    }

    public static WebApplication Build<T>(this WebApplicationBuilder builder) where T : Startup
    {
        T startup = (T)StartupInstance;

        var app = builder.Build();
        startup.Configure(app, app, builder.Environment);
        
        return app;
    }
}