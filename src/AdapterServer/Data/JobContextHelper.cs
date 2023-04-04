using System.Security.Claims;
using TaskQueueing.Persistence;

namespace AdapterServer.Data;

public static class JobContextHelper
{
    public static ClaimsPrincipal PrincipalFromString(string who)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,who)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "admin"));
    }
}
