using AuthenticationExtesion.AWS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace AuthenticationExtesion.Support;

public class UserGroupToRoleClaimsTransformation : IClaimsTransformation, IDisposable
{
    private RoleMappingService RoleMappingAccess { get; init; }
    private IOptionsMonitor<LoadBalancerAuthenticationOptions> OptionsMonitor { get; init; }
    private LoadBalancerAuthenticationOptions Options { get; set; }
    private IDisposable? _optionsListenerRegistration = null;
    private ILogger<UserGroupToRoleClaimsTransformation> Logger { get; init; }

    public UserGroupToRoleClaimsTransformation(RoleMappingService roleMappingAccess,
        IOptionsMonitor<LoadBalancerAuthenticationOptions> options,
        ILogger<UserGroupToRoleClaimsTransformation> logger)
    {
        RoleMappingAccess = roleMappingAccess;
        OptionsMonitor = options;
        _optionsListenerRegistration = options.OnChange(UpdateOptions);
        // TODO: how will this work if the config in Program.cs uses a different Scheme name?
        Options = options.Get(AwsLoadBalancerDefaults.AuthenticationScheme);
        Logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Do not map if unauthenticated
        if (!principal.Identities.Any(i => i.IsAuthenticated)) return Task.FromResult(principal);
        
        if (Options.ApplicationId is null)
        {
            Logger.LogWarning("AWS Authentication options not configured with application ID, cannot map roles.");
            return Task.FromResult(principal);
        }

        foreach (var identity in GetIdentitiesWithMappableRoles(principal))
        {
            var sourceIssuer = identity.FindFirst(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? "";

            var mappings = RoleMappingAccess.GetRoleMappingsBetweenSystems(sourceIssuer, Options.ApplicationId);
            Logger.LogTrace("Found {Count} possible role mappings for {Source}", mappings.Count(), sourceIssuer);

            if (!mappings.Any()) continue; // nothing to map

            ClaimsIdentity mappedIdentity = new();
            mappedIdentity.Label ??= $"Mapped Roles of {sourceIssuer}";

            foreach (var (Source, Target) in mappings)
            {
                if (identity.HasClaim(identity.RoleClaimType, Source.IdInSource)
                    && !principal.Identities.Any(i => i.HasClaim(i.RoleClaimType, Target.IdInSource))
                    && !mappedIdentity.HasClaim(mappedIdentity.RoleClaimType, Target.IdInSource))
                {
                    // ensure we do not add a duplicate claim (in the new identity nor any other identity)
                    mappedIdentity.AddClaim(new Claim(mappedIdentity.RoleClaimType, Target.IdInSource));
                    Logger.LogTrace("Mapped role {Source} to {Target} for user {User}", Source.IdInSource, Target.IdInSource, identity.Name);
                }
            }

            if (mappedIdentity.Claims.Any()) principal.AddIdentity(mappedIdentity);
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<ClaimsIdentity> GetIdentitiesWithMappableRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(c => c.Type == JwtRegisteredClaimNames.Iss)
                .Select(c => c.Subject)
                .Distinct()
                .Where(i => i is not null && i.IsAuthenticated && i.HasClaim(c => c.Type == i.RoleClaimType))
                .Cast<ClaimsIdentity>()
                .ToArray();
    }

    private void UpdateOptions(LoadBalancerAuthenticationOptions options, string? name)
    {
        Options = options;
    }

    public void Dispose()
    {
        _optionsListenerRegistration?.Dispose();
        _optionsListenerRegistration = null;
        GC.SuppressFinalize(this);
    }
}