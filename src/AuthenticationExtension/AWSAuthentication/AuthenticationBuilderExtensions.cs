using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AuthenticationExtesion.AWS;

public static class AwsLoadBalancerDefaults
{
    public const string AuthenticationScheme = "AwsLoadBalancerAuthentication";
}

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddAwsLoadBalancerAuthentication(this AuthenticationBuilder builder, Action<LoadBalancerAuthenticationOptions>? configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<LoadBalancerAuthenticationOptions>, LoadBalancerAuthenticationConfigureOptions>());
        return builder
            .AddScheme<LoadBalancerAuthenticationOptions, LoadBalancerAuthenticationHandler>(
                AwsLoadBalancerDefaults.AuthenticationScheme,
                configureOptions: configureOptions is null ? null : opts => configureOptions(opts)
            );
    }
}