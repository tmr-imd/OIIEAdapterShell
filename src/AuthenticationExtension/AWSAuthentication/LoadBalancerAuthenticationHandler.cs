using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
// using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationExtesion.AWS;

public class LoadBalancerAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// If provided, the unauthorized challenge will cause redirect to this URL
    /// instead of the default Forbidden. Useful when allowing a backup
    /// authentication mechanism such as a local login page.
    /// </summary>
    public string? RedirectUrlOnChallenge { get; set; }

    public string? AwsRegion { get; set; }

    /// <summary>
    /// URI (file or HTTP(S)) at which the public key can be retrieved.
    /// </summary>
    /// <remarks>
    /// The public key at the URI is expected to be in PEM format.
    /// </remarks>
    public string? PublicKeyUri { get; set; }

    public string? ValidIssuerForAccessToken { get; set; }

    public string? ValidIssuerForClaimsToken { get; set; }

    public string[] ValidAudiences { get; set; } = Array.Empty<string>();
    
    [DefaultValue(new [] {"ES256"})]
    public string[] ValidAlgorithms { get; set; } = new[] { "ES256" }; // AWS Docs say this is it, but in case the change
}

internal sealed class LoadBalancerAuthenticationConfigureOptions : IConfigureNamedOptions<LoadBalancerAuthenticationOptions>
{
    private const string ParentSections = "Authentication:Schemes";
    private readonly IConfiguration _configuration;

    public LoadBalancerAuthenticationConfigureOptions(IConfiguration configuration)
    {
        // Dotnet core 7 and above can switch to using IAuthenticationProvider
        _configuration = configuration;
    }

    public void Configure(string? name, LoadBalancerAuthenticationOptions options)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        var config = _configuration.GetSection($"{ParentSections}:{name}");

        if (config is null || !config.GetChildren().Any()) return;

        var algorithms = config.GetSection(nameof(options.ValidAlgorithms)).GetChildren().Select(c => c.Value);
        var audiences = config.GetSection(nameof(options.ValidAudiences)).GetChildren().Select(c => c.Value);
        // Do we want/need multiple possible issuers similar to JwtBearer authentication?
        // var issuers = config.GetSection(nameof(options.ValidAlgorithms)).GetChildren().Select(c => c.Value);

        // Merge the options with the configuration
        options.AwsRegion = config[nameof(options.AwsRegion)] ?? options.AwsRegion;
        options.ClaimsIssuer = config[nameof(options.ClaimsIssuer)] ?? options.ClaimsIssuer;
        options.ForwardAuthenticate = config[nameof(options.ForwardAuthenticate)] ?? options.ForwardAuthenticate;
        options.ForwardChallenge = config[nameof(options.ForwardChallenge)] ?? options.ForwardChallenge;
        options.ForwardDefault = config[nameof(options.ForwardDefault)] ?? options.ForwardDefault;
        options.ForwardForbid = config[nameof(options.ForwardForbid)] ?? options.ForwardForbid;
        options.ForwardSignIn = config[nameof(options.ForwardSignIn)] ?? options.ForwardSignIn;
        options.ForwardSignOut = config[nameof(options.ForwardSignOut)] ?? options.ForwardSignOut;
        options.PublicKeyUri = config[nameof(options.PublicKeyUri)] ?? options.PublicKeyUri;
        options.RedirectUrlOnChallenge = config[nameof(options.RedirectUrlOnChallenge)] ?? options.RedirectUrlOnChallenge;
        options.ValidAlgorithms = algorithms.Any() ? algorithms.ToArray() : options.ValidAlgorithms;
        options.ValidAudiences = audiences.Any() ? audiences.ToArray() : options.ValidAudiences;
        options.ValidIssuerForAccessToken = config[nameof(options.ValidIssuerForAccessToken)] ?? options.ValidIssuerForAccessToken;
        options.ValidIssuerForClaimsToken = config[nameof(options.ValidIssuerForClaimsToken)] ?? options.ValidIssuerForClaimsToken;
    }

    public void Configure(LoadBalancerAuthenticationOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}

/// <summary>
/// Authentication handler that processes AWS Elastic Load Balancer headers for
/// OIDC authentication, i.e., where the load balancer handles the OIDC flows
/// before it reaches the application.
/// </summary>
/// <remarks>
/// Refer to https://docs.aws.amazon.com/elasticloadbalancing/latest/application/listener-authenticate-users.html
/// </remarks>
/// <remarks>
/// Note: cannot provide a meaninggful challenge as the authentication process is
/// already completed by the load balancer before it reaches this point.
/// </remarks>
public class LoadBalancerAuthenticationHandler : AuthenticationHandler<LoadBalancerAuthenticationOptions>, IAuthenticationHandler
{
    /// <summary>
    /// Header field storing the access token from the token endpoint, in plain text.
    /// </summary>
    public const string ACCESS_TOKEN_HEADER = "x-amzn-oidc-accesstoken";
    /// <summary>
    /// Header field storing the subject field (sub) from the user info endpoint, in plain text.
    /// Used to identify the user.
    /// </summary>
    public const string IDENTITY_HEADER = "x-amzn-oidc-identity";
    /// <summary>
    /// Header field storing the user claims as a JWT.
    /// </summary>
    public const string CLAIMS_HEADER = "x-amzn-oidc-data";

    private const string SIGNER_PATTERN = "arn:aws:elasticloadbalancing:{region-code}:{account-id}:loadbalancer/app/{load-balancer-name}/{load-balancer-id}";
    private const string PUBLIC_KEY_ENDPOINT_PATTERN = "https://public-keys.auth.elb.{region}.amazonaws.com/{key-id}";

    public LoadBalancerAuthenticationHandler(IOptionsMonitor<LoadBalancerAuthenticationOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string oidcAccessToken = Request.Headers[ACCESS_TOKEN_HEADER].FirstOrDefault("");
        string oidcIdentity = Request.Headers[IDENTITY_HEADER].FirstOrDefault("");
        string oidcClaimsData = Request.Headers[CLAIMS_HEADER].FirstOrDefault("");

        Logger.LogTrace("Authenticiation with {Scheme}@{Endpoint}: {User}", 
            Scheme.Name,
            Request.HttpContext.GetEndpoint()?.DisplayName ?? "Unknown endpoint",
            oidcIdentity);

        var handler = new JsonWebTokenHandler();
        Logger.LogTrace("Access token details: {AccessToken}", oidcAccessToken.Split(".").Select(e => Base64UrlEncoder.Decode(e)).Take(2));
        Logger.LogTrace("Claims token details: {ClaimsToken}", oidcClaimsData.Split(".").Select(e => Base64UrlEncoder.Decode(e)).Take(2));

        if (string.IsNullOrWhiteSpace(oidcAccessToken) || string.IsNullOrWhiteSpace(oidcIdentity) || string.IsNullOrWhiteSpace(oidcClaimsData))
        {
            Logger.LogWarning("No AWS Load Balancer HTTP Headers set: {Headers}", Request.Headers.Keys);
            return AuthenticateResult.NoResult();
        }

        var oidcClaims = await DecodeAwsJwt(oidcClaimsData);

        if (!oidcClaims.IsAuthenticated || !oidcClaims.Claims.Any())
        {
            return AuthenticateResult.Fail("Invalid claims token.");
        }

        if (!oidcClaims.HasClaim(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == oidcIdentity))
        {
            return AuthenticateResult.Fail("Identity header value does not match 'sub' claim of the claims token.");
        }

        var principal = new ClaimsPrincipal(oidcClaims);

        // TODO: Verify the issue of the oidcAccessToken (which should be the original OIDC Identity Provider)

        Logger.LogInformation("Successfully authenticated {Name} ({Id})",
            principal.Identity?.Name, principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, this.Scheme.Name));
    }

    private async Task<ClaimsIdentity> DecodeAwsJwt(string encodedJwt)
    {
        // The manual way (matches example in AWS documentation)
        // var encodedJwtHeaders = encodedJwt.Split(".").First();
        // var decodedJwtHeaders = Base64UrlEncoder.Decode(encodedJwtHeaders);

        // Using the C# libraries (I am assuming it does the base64Url decoding)
        var handler = new JsonWebTokenHandler();

        var jwtHeaders = handler.ReadJsonWebToken(encodedJwt);
        var keyUri = GetPublicKeyUri(jwtHeaders);
        var publicKey = await GetPublicKey(keyUri);

        Logger.LogTrace("Validating token\n{Token}", jwtHeaders);

        var result = handler.ValidateToken(encodedJwt, new TokenValidationParameters {
            ValidIssuer = Options.ValidIssuerForClaimsToken,
            ValidAudiences = Options.ValidAudiences,
            ValidAlgorithms = Options.ValidAlgorithms,
            IssuerSigningKey = publicKey
        });

        if (result.IsValid && result.SecurityToken is JsonWebToken token)
        {
            Logger.LogTrace(
                "AWS Load Balancer provided claims token is valid for {Subj} - {Name}",
                result.ClaimsIdentity.FindFirst(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value,
                result.ClaimsIdentity.Name
            );
            return result.ClaimsIdentity;
        }
        else
        {
            Logger.LogWarning(result.Exception, "AWS Load Balancer provided claims token is invalid");
            return new ClaimsIdentity();
        }

        // var handler = new JwtSecurityTokenHandler();
        // var jwtHeaders = handler.ReadJwtToken(encodedJwt);
        // var keyUri = GetPublicKeyUri(jwtHeaders);
        // var publicKey = await GetPublicKey(keyUri);
        // var principal = handler.ValidateToken(encodedJwt, new TokenValidationParameters() {
        //     IssuerSigningKey = publicKey
        // }, out var decodedJwt);
        // using var scope = Logger.BeginScope("AWS JWT - Comparing principal vs. JWT");
        // Logger.LogInformation("ClaimsPrincipal {Principal}", principal);
        // Logger.LogInformation("JWT Content {JWT}", decodedJwt);
        // return principal.Identity as ClaimsIdentity;
    }

    // private Uri GetPublicKeyUri(JwtSecurityToken encryptedToken)
    // {
    //     if (Options.PublicKeyUri is not null) return new Uri(Options.PublicKeyUri);

    //     var url = PUBLIC_KEY_ENDPOINT_PATTERN.Replace("{region}", Options.AwsRegion).Replace("{key-id}", encryptedToken.Header.Kid);
    //     return new Uri(url);
    // }

    private Uri GetPublicKeyUri(JsonWebToken encryptedToken)
    {
        if (Options.PublicKeyUri is not null)
        {
            // Assume it is a file path if it does not start with the 'file:' scheme
            return Options.PublicKeyUri.StartsWith("file:") ?
                new Uri(Options.PublicKeyUri) :
                new Uri($"file:///{Path.GetFullPath(Options.PublicKeyUri).TrimStart('/')}");
        }

        var url = PUBLIC_KEY_ENDPOINT_PATTERN.Replace("{region}", Options.AwsRegion).Replace("{key-id}", encryptedToken.Kid);
        return new Uri(url);
    }

    private async Task<SecurityKey> GetPublicKey(Uri uri)
    {
        Logger.LogInformation("AWS JWT Processing - Reading Public Key from {URI}", uri);
        // TODO Error handling
        // TODO: key pinning/caching
        string keyString;
        if (uri.IsFile)
        {
            // More for testing and development, but could also be key pinning.
            // Since Uri.LocalPath actually always provides a Windows path (i.e., with backslashes)
            var filePath = Path.Combine(uri.Segments.Select(p => Uri.UnescapeDataString(p)).ToArray());
            Logger.LogInformation("Converted file path {Path}", filePath);
            keyString = await File.ReadAllTextAsync(filePath);
        }
        else
        {
            using var client = new HttpClient();
            keyString = await client.GetStringAsync(uri.AbsoluteUri);
        }

        Logger.LogInformation("Downloaded public key:\n{KeyData}", keyString);
        var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        key.ImportFromPem(keyString);
        return new ECDsaSecurityKey(key);
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