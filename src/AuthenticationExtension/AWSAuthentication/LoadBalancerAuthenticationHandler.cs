using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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

    /// <summary>
    /// AWS Region used to infer the load balancer's public key URL.
    /// </summary>
    /// <remarks>
    /// The public key at the derived URI is expected to be in PEM format.
    /// The LoadBalancerPublicKeyUri overrides this value.
    /// Is this one necessary if we have put the dynamic key-id bit into the other parameter?
    /// </remarks>
    public string? AwsRegion { get; set; }

    /// <summary>
    /// URI (file or HTTP(S)) at which the public key for the claims token can be retrieved.
    /// If the value includes a the text '{key-id}' it will be replaced with the kid of the JWT's header.
    /// </summary>
    /// <remarks>
    /// The public key at the URI is expected to be in PEM format.
    /// </remarks>
    public string? LoadBalancerPublicKeyUri { get; set; }

    /// <summary>
    /// URI (file or HTTP(S)) at which the public key(s) for the access token can be retrieved.
    /// </summary>
    /// <remarks>
    /// The location is expected to be a valid JSON Web Key Set (JWKS) document.
    /// </remarks>
    public string? IdPJwksUri { get; set; }

    /// <summary>
    /// List of strings identifier the issuers that will be considered valid for the access token.
    /// </summary>
    /// <remarks>
    /// The JSON may have a single string value or an array of string values.
    /// </remarks>
    public string[] ValidIssuersForAccessToken { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of strings identifier the issuers that will be considered valid for the access token.
    /// </summary>
    /// <remarks>
    /// The JSON may have a single string value or an array of string values.
    /// </remarks>
    public string[] ValidIssuersForClaimsToken { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of audiences that will be considered valid for the access token.
    /// If no value is given (empty array), no audience validation will be performed.
    /// </summary>
    /// <remarks>
    /// The JSON may have a single string value or an array of string values.
    /// </remarks>
    public string[] ValidAudiences { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The OAuth2/OIDC Application/Cient ID configured with the Identity Provider.
    /// </summary>
    /// <remarks>
    /// Helps protect against against reusing tokens captured from a nother application.
    /// If configured and no appid/azp claim is included in the token, it will be considered an error.
    /// </remarks>
    public string? ApplicationId { get; set; }

    /// <summary>
    /// The Tenant ID associated with the Identity Provided: IdP specific, e.g., Active Directory.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// List of valid algorithms for the claims token.
    /// </summary>
    /// <remarks>
    /// AWS Docs say it is always ES256, but may change in the future.
    /// </remarks>
    [DefaultValue(new [] {"ES256"})]
    public string[] ValidClaimsTokenAlgorithms { get; set; } = new[] { "ES256" };
    
    /// <summary>
    /// List of valid algorithms for the access token.
    /// </summary>
    /// <remarks>
    /// Identity provider dependent. By default only supporting 2 most common at the moment.
    /// </remarks>
    [DefaultValue(new [] {"RS256", "ES256"})]
    public string[] ValidAccessTokenAlgorithms { get; set; } = new[] { "RS256", "ES256" };

    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrWhiteSpace(IdPJwksUri)) throw new ValidationException($"{nameof(LoadBalancerAuthenticationOptions)}.{nameof(IdPJwksUri)} must not be null or empty");
        if (ValidClaimsTokenAlgorithms.Single() != "ES256") throw new ValidationException($"{nameof(LoadBalancerAuthenticationOptions)}.{nameof(ValidClaimsTokenAlgorithms)} contains unsupported algorithms {string.Join(", ", ValidClaimsTokenAlgorithms.Except(new[] {"ES256"}))}");
        if (ValidAccessTokenAlgorithms.Except(new[] { "RS256", "ES256" }).Any()) throw new ValidationException($"{nameof(LoadBalancerAuthenticationOptions)}.{nameof(ValidAccessTokenAlgorithms)} contains unsupported algorithms {string.Join(", ", ValidAccessTokenAlgorithms.Except(new[] {"RS256", "ES256"}))}");
    }
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

        var claimsTokenAlgorithms = GetValueOrChildren(config.GetSection(nameof(options.ValidClaimsTokenAlgorithms)));
        var accessTokenAlgorithms = GetValueOrChildren(config.GetSection(nameof(options.ValidAccessTokenAlgorithms)));
        var audiences = GetValueOrChildren(config.GetSection(nameof(options.ValidAudiences)));
        var accessTokenIssuers = GetValueOrChildren(config.GetSection(nameof(options.ValidIssuersForAccessToken)));
        var claimsTokenIssuers = GetValueOrChildren(config.GetSection(nameof(options.ValidIssuersForClaimsToken)));

        // Merge the options with the configuration
        options.AwsRegion = config[nameof(options.AwsRegion)] ?? options.AwsRegion;
        options.ClaimsIssuer = config[nameof(options.ClaimsIssuer)] ?? options.ClaimsIssuer;
        options.ForwardAuthenticate = config[nameof(options.ForwardAuthenticate)] ?? options.ForwardAuthenticate;
        options.ForwardChallenge = config[nameof(options.ForwardChallenge)] ?? options.ForwardChallenge;
        options.ForwardDefault = config[nameof(options.ForwardDefault)] ?? options.ForwardDefault;
        options.ForwardForbid = config[nameof(options.ForwardForbid)] ?? options.ForwardForbid;
        options.ForwardSignIn = config[nameof(options.ForwardSignIn)] ?? options.ForwardSignIn;
        options.ForwardSignOut = config[nameof(options.ForwardSignOut)] ?? options.ForwardSignOut;
        options.LoadBalancerPublicKeyUri = config[nameof(options.LoadBalancerPublicKeyUri)] ?? options.LoadBalancerPublicKeyUri;
        options.IdPJwksUri = config[nameof(options.IdPJwksUri)] ?? options.IdPJwksUri;
        options.RedirectUrlOnChallenge = config[nameof(options.RedirectUrlOnChallenge)] ?? options.RedirectUrlOnChallenge;
        options.ApplicationId = config[nameof(options.ApplicationId)] ?? options.ApplicationId;
        options.TenantId = Guid.TryParse(config[nameof(options.TenantId)], out var guid) ? guid : options.TenantId;
        options.ValidClaimsTokenAlgorithms = claimsTokenAlgorithms.Any() ? claimsTokenAlgorithms.ToArray() : options.ValidClaimsTokenAlgorithms;
        options.ValidAccessTokenAlgorithms = accessTokenAlgorithms.Any() ? accessTokenAlgorithms.ToArray() : options.ValidAccessTokenAlgorithms;
        options.ValidAudiences = audiences.Any() ? audiences.ToArray() : options.ValidAudiences;
        options.ValidIssuersForAccessToken = accessTokenIssuers.Any() ? accessTokenIssuers.ToArray() : options.ValidIssuersForAccessToken;
        options.ValidIssuersForClaimsToken = claimsTokenIssuers.Any() ? claimsTokenIssuers.ToArray() : options.ValidIssuersForClaimsToken;
    }

    public void Configure(LoadBalancerAuthenticationOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    private static IEnumerable<string> GetValueOrChildren(IConfigurationSection section)
    {
        return section.Value != null ? new string[] { section.Value } : section.GetChildren().Select(c => c.Value);
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
    private const string RoleTypeClaimType = "roles";
    private const string TenantIdClaimType = "tid";
    private const string AppIdClaimType = "appid";

    private TokenValidationParameters ClaimsTokenValidationParameters => new()
    {
        ValidIssuers = Options.ValidIssuersForClaimsToken,
        ValidateAudience = false,
        ValidAlgorithms = Options.ValidClaimsTokenAlgorithms,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        AuthenticationType = AwsLoadBalancerDefaults.AuthenticationScheme
    };

    private TokenValidationParameters AccessTokenValidationParameters => new()
    {
        ValidIssuers = Options.ValidIssuersForAccessToken,
        ValidateAudience = Options.ValidAudiences.Any(),
        ValidAudiences = Options.ValidAudiences,
        ValidAlgorithms = Options.ValidAccessTokenAlgorithms,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        AuthenticationType = AwsLoadBalancerDefaults.AuthenticationScheme
    };

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

        var oidcClaims = await DecodeAwsJwt(oidcClaimsData, ClaimsTokenValidationParameters, GetClaimsPublicKeyUri);

        if (!oidcClaims.IsAuthenticated || !oidcClaims.Claims.Any())
        {
            return AuthenticateResult.Fail("Invalid claims token.");
        }

        if (!oidcClaims.HasClaim(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == oidcIdentity))
        {
            return AuthenticateResult.Fail("Identity header value does not match 'sub' claim of the claims token.");
        }

        oidcClaims.AddClaim(new Claim(ClaimTypes.NameIdentifier, oidcIdentity));
        var principal = new ClaimsPrincipal(oidcClaims);

        var accessClaims = await DecodeAwsJwt(oidcAccessToken, AccessTokenValidationParameters, GetAccessTokenKeyUri);
        // validate extra Access Token specific claims (ApplicationId, TenantId)
        if (!accessClaims.IsAuthenticated || !accessClaims.Claims.Any())
        {
            return AuthenticateResult.Fail("Invalid access token.");
        }
        else if (!VerifyAdditionalAccessTokenRequirements(accessClaims))
        {
            return AuthenticateResult.Fail("Invalid access token (additional requirements failed verification).");
        }

        // TODO: Should we add all the claims or only the 'roles'/'groups' claims?
        principal.AddIdentity(accessClaims);

        Logger.LogInformation("Successfully authenticated {Name} ({Id})",
            principal.Identity?.Name, principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, this.Scheme.Name));
    }

    private async Task<ClaimsIdentity> DecodeAwsJwt(string encodedJwt,
            TokenValidationParameters validationParameters,
            Func<JsonWebToken, Uri> PublicKeyResolver)
    {
        var handler = new JsonWebTokenHandler();

        var jwtHeaders = handler.ReadJsonWebToken(encodedJwt);
        var keyUri = PublicKeyResolver(jwtHeaders);
        var publicKey = await GetPublicKey(keyUri, jwtHeaders);

        if (publicKey is null) return new ClaimsIdentity();
        validationParameters.IssuerSigningKey = publicKey;

        Logger.LogTrace("Validating token\n{Token}", jwtHeaders);

        var result = handler.ValidateToken(encodedJwt, validationParameters);

        if (result.IsValid && result.SecurityToken is JsonWebToken token)
        {
            Logger.LogTrace(
                "AWS Load Balancer provided claims token is valid for {Subj} - {Name}",
                token.Subject,
                token.GetClaim(JwtRegisteredClaimNames.Name).Value
            );
            var claimsIdentity = result.ClaimsIdentity;
            claimsIdentity = new ClaimsIdentity(claimsIdentity.Claims, claimsIdentity.AuthenticationType, JwtRegisteredClaimNames.Name, RoleTypeClaimType)
            {
                Label = claimsIdentity.Label,
                BootstrapContext = claimsIdentity.BootstrapContext
            };
            return claimsIdentity;
        }
        else
        {
            Logger.LogWarning(result.Exception, "AWS Load Balancer provided claims token is invalid");
            return new ClaimsIdentity();
        }
    }

    private Uri GetClaimsPublicKeyUri(JsonWebToken jwt)
    {
        var url = string.IsNullOrWhiteSpace(Options.LoadBalancerPublicKeyUri) ? PUBLIC_KEY_ENDPOINT_PATTERN : Options.LoadBalancerPublicKeyUri;
        url = url.Replace("{region}", Options.AwsRegion).Replace("{key-id}", jwt.Kid);

        // Assume it is a file path if it does not start with the 'file:', 'http:', or 'https:' schemes
        return url.StartsWith("file:") || url.StartsWith("http:") || url.StartsWith("https:") ?
            new Uri(url) :
            new Uri($"file:///{Path.GetFullPath(url).TrimStart('/')}");
    }

    private Uri GetAccessTokenKeyUri(JsonWebToken jwt)
    {
        // Future option may be to construct the JWKS uri from the issuer, but could be ureliable
        if (string.IsNullOrWhiteSpace(Options.IdPJwksUri)) return new Uri("", UriKind.Relative);
        var url = Options.IdPJwksUri;

        // Assume it is a file path if it does not start with the 'file:', 'http:', or 'https:' schemes
        return url.StartsWith("file:") || url.StartsWith("http:") || url.StartsWith("https:") ?
            new Uri(url) :
            new Uri($"file:///{Path.GetFullPath(url).TrimStart('/')}");
    }

    private async Task<SecurityKey?> GetPublicKey(Uri uri, JsonWebToken jwt)
    {
        Logger.LogInformation("AWS JWT Processing - Reading Public Key from {URI}", uri);
        // TODO: key pinning/caching
        string keyString = uri.IsFile ? await GetPublicKeyFromFile(uri) : await GetPublicKeyFromHttp(uri);

        Logger.LogTrace("Downloaded public key:\n{KeyData}", keyString);

        if (string.IsNullOrWhiteSpace(keyString))
        {
            Logger.LogError("No public key data retrieved {URL}", uri);
            return null;
        }

        if (PemEncoding.TryFind(keyString, out var pemInfo))
        {
            return ReadPemPublicKey(keyString[pemInfo.Label], keyString[pemInfo.Base64Data]);
        }
        else if (keyString.TrimStart().StartsWith('{'))
        {
            return ReadFromJwks(keyString, jwt.Kid);
        }
        else
        {
            Logger.LogError("Error reading public key data content. Expecting PEM Public Key or Json Web Key Set file.");
            return null;
        }
    }

    private async Task<string> GetPublicKeyFromFile(Uri uri)
    {
        // More for testing and development, but could also be key pinning.
        // Since Uri.LocalPath actually always provides a Windows path (i.e., with backslashes)
        var filePath = Path.Combine(uri.Segments.Select(p => Uri.UnescapeDataString(p)).ToArray());
        Logger.LogInformation("Converted file path {Path}", filePath);
        string keyString = await File.ReadAllTextAsync(filePath);
        return keyString;
    }

    private async Task<string> GetPublicKeyFromHttp(Uri uri)
    {
        try
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(uri.AbsoluteUri);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading public key from HTTP: {URL}", uri.AbsoluteUri);
            return "";
        }
    }

    private SecurityKey? ReadPemPublicKey(string pemLabel, string pemData)
    {
        if (pemLabel != "PUBLIC KEY")
        {
            Logger.LogError("Error reading public key data content. Expecting PEM Public Key, found {PemLabel}", pemLabel);
            return null;
        }

        var decoded = Convert.FromBase64String(pemData);
        var publicKey = PublicKey.CreateFromSubjectPublicKeyInfo(decoded, out var _);

        if (publicKey.GetECDsaPublicKey() is ECDsa ecdsa)
        {
            return new ECDsaSecurityKey(ecdsa);
        }
        else if (publicKey.GetRSAPublicKey() is RSA rsa)
        {
            return new RsaSecurityKey(rsa);
        }

        return null;
    }

    private SecurityKey? ReadFromJwks(string json, string keyId)
    {
        try
        {
            var jwks = JsonWebKeySet.Create(json);
            var jwk = jwks.Keys.Where(k => k.KeyId == keyId).SingleOrDefault() ?? throw new Exception($"No matching Kid (Key Id): {keyId}.");

            if (!string.IsNullOrWhiteSpace(jwk.Alg) && (!jwk.IsSupportedAlgorithm(jwk.Alg) || !Options.ValidAccessTokenAlgorithms.Contains(jwk.Alg)))
                throw new Exception($"Unsupported algorithm {jwk.Alg}");

            if (!string.IsNullOrWhiteSpace(jwk.Alg) && jwk.Use != JsonWebKeyUseNames.Sig)
                throw new Exception("Key use is not for signatures ('sig')");

            if (jwk.AdditionalData.ContainsKey("issuer") && !Options.ValidIssuersForAccessToken.Contains(jwk.AdditionalData["issuer"]))
                throw new Exception($"Key issuer is not one of the valid issuers for access tokens: {jwk.AdditionalData["issuer"]}");

            return jwk.X5c.Any() ? GetKeyFromCertificateChain(jwk) : GetKeyFromParameters(jwk);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading public key data content from JWKS.");
            return null;
        }
    }

    private static SecurityKey GetKeyFromParameters(JsonWebKey jwk) =>
        jwk.Kty switch
        {
            JsonWebAlgorithmsKeyTypes.EllipticCurve => new ECDsaSecurityKey(ECDsa.Create(new ECParameters
            {
                // TODO: confirm compaitbility of the JsonWebKey Crv values and the 'Friendly Names' of ECCurve.
                Curve = ECCurve.CreateFromFriendlyName(jwk.Crv),
                Q = new ECPoint {
                    X = Convert.FromBase64String(jwk.X),
                    Y = Convert.FromBase64String(jwk.Y)
                }
            })),
            JsonWebAlgorithmsKeyTypes.RSA => new RsaSecurityKey(new RSAParameters
            {
                Exponent = Convert.FromBase64String(jwk.E),
                Modulus = Convert.FromBase64String(jwk.N)
            }),
            _ => throw new Exception($"Unexpected key type/algorithm family: expected RSA or EC but got {jwk.Kty}"),
        };

    private static SecurityKey GetKeyFromCertificateChain(JsonWebKey jwk)
    {
        var certificateChain = new X509Certificate2Collection(jwk.X5c.Select(c => new X509Certificate2(Convert.FromBase64String(c))).ToArray());
        // The simple verify does not make it clear why the failure occurs, so is not useful.
        // if (!certificateChain[0].Verify()) throw new Exception("JWKS Certificate Chain failed verification.");

        if (certificateChain[0].GetRSAPublicKey() is RSA rsa)
        {
            return new RsaSecurityKey(rsa);
        }
        else if (certificateChain[0].GetECDsaPublicKey() is ECDsa ecdsa)
        {
            return new ECDsaSecurityKey(ecdsa);
        }
        else
        {
            throw new Exception("JWKS Certificate does not contain an RSA nor ECDsa public key.");
        }
    }

    private bool VerifyAdditionalAccessTokenRequirements(ClaimsIdentity accessClaims)
    {
        try
        {
            if (Options.TenantId is not null) VerifyAccessTokenTenantId(accessClaims);
            if (string.IsNullOrWhiteSpace(Options.ApplicationId)) VerifyAccessTokenApplicationId(accessClaims);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Access token failed validation.");
            return false;
        }
    }

    private void VerifyAccessTokenTenantId(ClaimsIdentity accessClaims)
    {
        if (accessClaims.FindFirst(c => c.Type == TenantIdClaimType) is Claim tenantId)
        {
            var isGuid = Guid.TryParse(tenantId.Value, out var tenantGuid);
            if (!isGuid) throw new Exception($"Tenant ID claim ('{TenantIdClaimType}') is not a valid GUID");
            if (Options.TenantId != tenantGuid) throw new Exception($"Tenant ID ('{TenantIdClaimType}') claim does not match. Received {tenantGuid}");

            var issuer = accessClaims.FindFirst(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? throw new Exception("Access token issuer not available in resulting claims.");
            var issuerUri = new Uri(issuer);
            string tenantIdString = Options.TenantId.ToString()!;
            if (issuerUri.Segments.Last() == tenantIdString || issuerUri.Segments.TakeLast(2).SequenceEqual(new string[] {tenantIdString, "v2.0"}))
                throw new Exception($"Access token issuer does not include expected Tenant ID. Received {issuer}");
        }
        else
        {
            throw new Exception($"Tenant ID ('{TenantIdClaimType}') claim is missing");
        }
    }

    private void VerifyAccessTokenApplicationId(ClaimsIdentity accessClaims)
    {
        // Application ID 'appid' 'azp'
        if (accessClaims.FindFirst(c => c.Type == JwtRegisteredClaimNames.Azp || c.Type == AppIdClaimType) is Claim appId)
        {
            if (appId.Value != Options.ApplicationId)
                throw new Exception($"Application/Client ID ('{AppIdClaimType}' or '{JwtRegisteredClaimNames.Azp}') does not match expected. Received {appId.Value}");
        }
        else
        {
            throw new Exception($"Application/Client ID ('{AppIdClaimType}' or '{JwtRegisteredClaimNames.Azp}') is missing");
        }
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