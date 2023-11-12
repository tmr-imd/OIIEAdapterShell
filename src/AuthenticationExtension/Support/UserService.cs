using System.Security.Claims;

namespace AuthenticationExtesion.Support;

/// <summary>
/// Convenience service to provide the current user to the application consistently
/// across MVC, Razor, and Blazor components, and other services.
/// </summary>
/// <remarks>
/// See discussin of security in Blazor applications:
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/additional-scenarios?view=aspnetcore-7.0
/// </remarks>
public class UserService
{
    private ClaimsPrincipal currentUser = new(new ClaimsIdentity());

    public ClaimsPrincipal CurrentUser
    {
        get => currentUser;
        internal set
        {
            if (currentUser != value) currentUser = value;
        }
    }
}
