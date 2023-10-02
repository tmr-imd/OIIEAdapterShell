using Microsoft.AspNetCore.Authorization;

namespace AuthenticationExtesion.Support;

public static class DefaultAuthorizationPolicy
{
    public const string AUTHORIZATION_POLICY_NAME = "AdministratorsOnly";

    /// <summary>
    /// Adds an authorization policy for "AdministratorsOnly", optional delegate
    /// overrides the default policy settings.
    /// TODO: document the default policy.
    /// </summary>
    /// <param name="configurePolicy">Delegate to build the policy</param>
    public static void AddAdministratorsOnlyPolicy(this AuthorizationOptions options,
            Action<AuthorizationPolicyBuilder>? configurePolicy = null)
    {
        options.AddPolicy(AUTHORIZATION_POLICY_NAME, policy =>
        {
            if (configurePolicy is null)
            {
                // Default policy
                policy.RequireRole("FakeAdmin");
            }
            else
            {
                configurePolicy(policy);
            }
        });
    }
}