using CIRServices;
using CIRLib.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CIRLib.Extensions;

public static class ServiceCollectionsExtensions
{
    public static void AddCIRServices(this IServiceCollection services, IConfiguration config)
    {
        CIRLibContextFactory factory = new CIRLibContextFactory(config);
        CIRManager.Factory = factory;
        services.AddSingleton(factory);
        services.AddScoped<RegistryServices>();
        services.AddScoped<CategoryServices>();
        services.AddScoped<EntryServices>();
        services.AddScoped<PropertyServices>();
        services.AddScoped<PropertyValueServices>();
    }
}