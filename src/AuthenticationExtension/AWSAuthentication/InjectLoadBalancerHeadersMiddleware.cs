using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AuthenticationExtesion.AWS;

/// <summary>
/// For development mode only, will inject headers that look like the
/// AWS Load Balancer headers.
/// </summary>
public static class InjectLoadBalancerHeadersMiddleware
{
    public static IApplicationBuilder UseInjectLoadBalancerHeaders(this IApplicationBuilder builder)
    {
        return builder.Use(InjectLoadBalancerHeaders);
    }

    public static async Task InjectLoadBalancerHeaders(HttpContext context, RequestDelegate next)
    {
        context.Request.Headers["x-example"] = "Fake Header";
        await next(context);
    }
}