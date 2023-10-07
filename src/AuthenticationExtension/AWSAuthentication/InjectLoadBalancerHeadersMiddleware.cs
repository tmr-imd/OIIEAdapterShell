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
        // TODO get from configuration
        context.Request.Headers[LoadBalancerAuthenticationHandler.ACCESS_TOKEN_HEADER] = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmYWtlVXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJGYWtlIFVzZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJGYWtlIFJvbGUiLCJhdWQiOiJUQkQiLCJleHAiOjE3Mjc3NzIzODEsImlzcyI6IlRCRCIsImlhdCI6MTY5NjIzNjM4MSwibmJmIjoxNjk2MjM2MzgxfQ.O9x4gRod1a0PrvY07pjjQSzqGQ4oAxf8YaDkBVDGYKrogLd8ioSyGSMzD1gw6HNyOK8BgcQkDb7LygBoC4v31Q";
        context.Request.Headers[LoadBalancerAuthenticationHandler.IDENTITY_HEADER] = "fakeUser";
        context.Request.Headers[LoadBalancerAuthenticationHandler.CLAIMS_HEADER] = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmYWtlVXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJGYWtlIFVzZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJGYWtlIFJvbGUiLCJhdWQiOiJUQkQiLCJleHAiOjE3Mjc3NzIzODEsImlzcyI6IlRCRCIsImlhdCI6MTY5NjIzNjM4MSwibmJmIjoxNjk2MjM2MzgxfQ.O9x4gRod1a0PrvY07pjjQSzqGQ4oAxf8YaDkBVDGYKrogLd8ioSyGSMzD1gw6HNyOK8BgcQkDb7LygBoC4v31Q";
        await next(context);
    }
}