using Microsoft.AspNetCore.Authentication;

namespace AuthenticationExtesion.AWS;

public static class AwsLoadBalancerDefaults
{
    public const string AuthenticationScheme = "AwsLoadBalancerAuthentication";
}

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddAwsLoadBalancerAuthentication(this AuthenticationBuilder builder, Action<LoadBalancerAuthenticationOptions> configureOptions)
    {
        return builder
            .AddScheme<LoadBalancerAuthenticationOptions, LoadBalancerAuthenticationHandler>(
                AwsLoadBalancerDefaults.AuthenticationScheme,
                opts => configureOptions(opts)
            );
    }
}