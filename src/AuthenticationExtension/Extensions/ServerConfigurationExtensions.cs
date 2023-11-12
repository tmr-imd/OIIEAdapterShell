using AuthenticationExtesion.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuthenticationExtesion.Extensions;

public static class ServerConfigurationExtensions
{
    public static void AddRoleMappingServices(this IServiceCollection services)
    {
        services.AddScoped<RoleMappingService>();
        services.AddTransient<IClaimsTransformation, UserGroupToRoleClaimsTransformation>();
    }

    public static void AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());
    }

    /// <summary>
    /// Adds middleware to update the UserService for MVC/Razor pages.
    /// </summary>
    /// <remarks>
    /// This must be called immediately before the call to MapBlazorHub.
    /// </remarks>
    public static void UseUserServiceMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserServiceMiddleware>();
    }
}