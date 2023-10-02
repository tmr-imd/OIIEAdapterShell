using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationExtesion.Support;

/// <summary>
/// An authentication handler that performs no authentication.
/// To be used to allow open access to some paths, e.g., the notification route.
/// </summary>
public class NoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationHandler
{
    public NoAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        // Could instantiate a special claims principal used for all api access, or could create a principal that captures the IP Address
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(this.Scheme.Name);
        identity.AddClaim(new Claim(ClaimTypes.Name, "Internal"));
        Logger.LogInformation("Bypassing authentication for {Identity}", identity.AuthenticationType);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, this.Scheme.Name)));
    }
}

public static class NoAuthenticationDefaults
{
    public const string AuthenticationScheme = "NoAuthentication";
}

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddNoAuthenticationScheme(this AuthenticationBuilder builder, Action<AuthenticationSchemeOptions> configureOptions)
    {
        return builder
            .AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(
                NoAuthenticationDefaults.AuthenticationScheme,
                opts => configureOptions(opts)
            );
    }
}
