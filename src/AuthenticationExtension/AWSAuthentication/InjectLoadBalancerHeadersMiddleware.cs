using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthenticationExtesion.AWS;

/// <summary>
/// For development mode only, will inject headers that look like the
/// AWS Load Balancer headers.
/// </summary>
public static class InjectLoadBalancerHeadersMiddleware
{
    public const string InjectLoadBalancerHeadersConfigKey = "AwsLoadBalancerHeaders";
    private static InjectLoadBalancerHeadersOptions? Options;

    public static IApplicationBuilder UseInjectLoadBalancerHeaders(this IApplicationBuilder builder)
    {
        // Force a failure if we are using the injection but config is not set
        Options = builder.ApplicationServices.GetRequiredService<IOptions<InjectLoadBalancerHeadersOptions>>().Value;
        if (string.IsNullOrWhiteSpace(Options.AccessToken) || string.IsNullOrWhiteSpace(Options.ClaimsToken) || string.IsNullOrWhiteSpace(Options.SubjectIdentity) )
            throw new Exception($@"Unable to add InjectLoadBalancerHeaders middleware as configuration parameters are not set: 
            {InjectLoadBalancerHeadersConfigKey}:{nameof(InjectLoadBalancerHeadersOptions.AccessToken)}
            {InjectLoadBalancerHeadersConfigKey}:{nameof(InjectLoadBalancerHeadersOptions.SubjectIdentity)}
            {InjectLoadBalancerHeadersConfigKey}:{nameof(InjectLoadBalancerHeadersOptions.ClaimsToken)}");

        return builder.Use(InjectLoadBalancerHeaders);
    }

    public static async Task InjectLoadBalancerHeaders(HttpContext context, RequestDelegate next)
    {
        context.Request.Headers[LoadBalancerAuthenticationHandler.ACCESS_TOKEN_HEADER] = Options?.AccessToken;
        context.Request.Headers[LoadBalancerAuthenticationHandler.IDENTITY_HEADER] = Options?.SubjectIdentity;
        context.Request.Headers[LoadBalancerAuthenticationHandler.CLAIMS_HEADER] = Options?.ClaimsToken;
        await next(context);
    }
}

public class InjectLoadBalancerHeadersOptions
{
    /// <summary>
    /// Base64Url Encoded JWT simulating the Access Token provided by the
    /// AWS Elastic Load Balancer
    /// </summary>
    public string? AccessToken { get; set; }
    /// <summary>
    /// The subject/identity for the access token and claims, simulating the value
    /// of the identity provided by the AWS Load Balancer
    /// </summary>
    public string? SubjectIdentity { get; set; }
    /// <summary>
    /// Base64Url Encoded JWT simulating the Claims Token provided by the
    /// AWS Elastic Load Balancer
    /// </summary>
    public string? ClaimsToken { get; set; }
}
