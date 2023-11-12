using Microsoft.AspNetCore.Http;

namespace AuthenticationExtesion.Support;

/// <summary>
/// Middleware to set the current user in the UserService for consistency
/// across MVC, Razor, and Blazor components, and other services.
/// </summary>
/// <remarks>
/// See discussin of security in Blazor applications:
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/additional-scenarios?view=aspnetcore-7.0
/// </remarks>
public class UserServiceMiddleware
{
    private readonly RequestDelegate next;

    public UserServiceMiddleware(RequestDelegate next)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        userService.CurrentUser = context.User;
        await next(context);
    }
}