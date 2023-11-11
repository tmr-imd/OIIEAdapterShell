using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AuthenticationExtesion.Support;

public class UserGroupToRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        Console.WriteLine("!!! Adding FakeAdmin role claim for {0}", principal.Identity?.Name);
        // TODO: probably need such a thing to translate AD groups into the local roles for authorization.
        ClaimsIdentity claimsIdentity = new();
        claimsIdentity.Label ??= "Mapped Roles";
        var claimType = ClaimTypes.Role;
        if (principal.Identities.Any(i => i.IsAuthenticated) &&
            !principal.HasClaim(claim => claim.Type == claimType && claim.Value == "FakeAdmin"))
        {
            // ensure we do not add a duplicate claim
            claimsIdentity.AddClaim(new Claim(claimType, "FakeAdmin"));
            principal.AddIdentity(claimsIdentity);
        }

        return Task.FromResult(principal);
    }
}