using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticationExtesion.AWS;

public class LoadBalancerAuthenticationOptions : AuthenticationSchemeOptions
{

}

public class LoadBalancerAuthenticationHandler : AuthenticationHandler<LoadBalancerAuthenticationOptions>, IAuthenticationHandler
{
    public LoadBalancerAuthenticationHandler(IOptionsMonitor<LoadBalancerAuthenticationOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation("Authenticiation with {Scheme}@{Endpoint}: {User}", 
            Scheme.Name, Request.HttpContext.GetEndpoint()?.DisplayName ?? "No display name",
            Request.Headers["x-example"]);
        Logger.LogWarning("Not yet implemented, returning failure.");
        return Task.FromResult(AuthenticateResult.Fail("AWS LoadBalancer Authentication not yet implemented."));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // May or may not need to override the default behaviour.
        // May want to change it to a 403 (Forbidden) instead of the default 401 (Unauthorized) challenge.
        // This is because the load balancer will have already performed any challenge.
        // Alternatively if we have received a direct connection not through the load balancer we may forward
        // to a different authentication scheme, maybe a local login page or something.
        return base.HandleChallengeAsync(properties);
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        // May or may not need to override the default behaviour.
        return base.HandleForbiddenAsync(properties);
    }
}