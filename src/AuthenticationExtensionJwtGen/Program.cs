using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;


IConfiguration Config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

TokenOptions Options = Config.Get<TokenOptions>();
if (Config.GetSection("Roles").Exists())
{
    // Use replace semantics for the Roles list instead of append.
    Options.Roles = Config.GetSection("Roles").Get<List<string>>();
}

Console.WriteLine("Using options {0}\n", Options);

var claimsKeyPath = "claims_pub_key.pem";
var accessKeyPath = "access_pub_key.pem";
var accesJwksPath = "access_jwks.json";
var claimsTokenPath = "claims_token.txt";
var accessTokenPath = "access_token.txt";

// Create new public key using same algorithm as AWS Load Balancer (ES256)
var rawKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var publicKeyBytes = rawKey.ExportSubjectPublicKeyInfo();
var encoded = PemEncoding.Write("EC PUBLIC KEY", publicKeyBytes);
File.WriteAllText(claimsKeyPath, new string (encoded));
Console.WriteLine("Claims token public key written to: {0}", claimsKeyPath);
// Console.WriteLine("{0}\n", new string (encoded));

// Create new public key for the access token, using RSA (RS256)
var rawRsaKey = RSA.Create(2048);
var rsaPublicKeyBytes = rawRsaKey.ExportSubjectPublicKeyInfo();
var rsaPublicKey = PemEncoding.Write("PUBLIC KEY", rsaPublicKeyBytes);
File.WriteAllText(accessKeyPath, new string(rsaPublicKey));
Console.WriteLine("Access token public key written to: {0}", accessKeyPath);
// Console.WriteLine("{0}\n", new string(rsaPublicKey));

// Generate a new JWT
var tokenHandler = new JsonWebTokenHandler();

var now = DateTime.UtcNow;

// Common claims

var identity = new ClaimsIdentity(new Claim[]
{
    new Claim(JwtRegisteredClaimNames.Sub, Options.UserIdentity),
    new Claim(JwtRegisteredClaimNames.Name, Options.UserFullName),
    new Claim(JwtRegisteredClaimNames.GivenName, Options.UserGivenName),
    new Claim(JwtRegisteredClaimNames.FamilyName, Options.UserFamilyName),
    new Claim(JwtRegisteredClaimNames.Email, Options.UserEmail)
});

// Claims Token

if (Options.IncludeRolesInUserData)
{
    foreach (var r in Options.Roles)
    {
        identity.AddClaim(new Claim("roles", r));
    }
}

var claimsSecurityKey = new ECDsaSecurityKey(rawKey);
claimsSecurityKey.KeyId = Base64UrlEncoder.Encode(claimsSecurityKey.ComputeJwkThumbprint());
var claimsSigningCredentials = new SigningCredentials(claimsSecurityKey, SecurityAlgorithms.EcdsaSha256);

string claimsToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
{
    Issuer = Options.ClaimsDataIssuer,
    Expires = now.AddDays(365),
    NotBefore = null,
    IssuedAt = null,
    Subject = identity,
    SigningCredentials = claimsSigningCredentials
});

File.WriteAllText(claimsTokenPath, claimsToken);
Console.WriteLine("Claims token written to: {0}", claimsTokenPath);

var jwtHeaders = tokenHandler.ReadJsonWebToken(claimsToken);

var decodedJwtHeaders = Base64UrlEncoder.Decode(jwtHeaders.EncodedHeader);
Console.WriteLine("Decoded claims token headers {0}", decodedJwtHeaders);

var decodedPayload = Base64UrlEncoder.Decode(jwtHeaders.EncodedPayload);
Console.WriteLine("Decoded claims token payload {0}", decodedPayload);

var validationResult = tokenHandler.ValidateToken(claimsToken, new TokenValidationParameters
{
    ValidIssuer = Options.ClaimsDataIssuer,
    ValidateAudience = false,
    IssuerSigningKey = claimsSecurityKey
});

if (!validationResult.IsValid) throw new Exception("Claims token verification failed with generated keys");
// Console.WriteLine("Claims token validation {0}", validationResult.IsValid);

// Access Token
Console.WriteLine();

if (!Options.IncludeRolesInUserData)
{
    foreach (var r in Options.Roles)
    {
        identity.AddClaim(new Claim("roles", r));
    }
}

var accessSecurityKey = new RsaSecurityKey(rawRsaKey);
accessSecurityKey.KeyId = Base64UrlEncoder.Encode(accessSecurityKey.ComputeJwkThumbprint());
var accessSigningCredentials = new SigningCredentials(accessSecurityKey, SecurityAlgorithms.RsaSha256);

var extraClaims = new Dictionary<string, object>()
{
    { Options.UseApzForAppId ? "apz" : "appid", Options.ApplicationId },
    { "tid", Options.UseTenantId ? Options.TenantId : null! }
}.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value);

string accessToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
{
    Issuer = Options.AccessTokenIssuer,
    Audience = Options.Audience,
    NotBefore = now,
    Expires = now.AddDays(365),
    IssuedAt = now,
    Subject = identity,
    Claims = extraClaims,
    SigningCredentials = accessSigningCredentials
});

File.WriteAllText(accessTokenPath, accessToken);
Console.WriteLine("Access token written to: {0}", accessTokenPath);

jwtHeaders = tokenHandler.ReadJsonWebToken(accessToken);

decodedJwtHeaders = Base64UrlEncoder.Decode(jwtHeaders.EncodedHeader);
Console.WriteLine("Decoded access token headers {0}", decodedJwtHeaders);

decodedPayload = Base64UrlEncoder.Decode(jwtHeaders.EncodedPayload);
Console.WriteLine("Decoded access token payload {0}", decodedPayload);

validationResult = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
{
    ValidIssuer = Options.AccessTokenIssuer,
    ValidAudiences = new string[] { Options.Audience },
    IssuerSigningKey = accessSecurityKey
});

if (!validationResult.IsValid) throw new Exception("Access token verification failed with generated keys");
// Console.WriteLine("Access token validation {0}", validationResult.IsValid);

// Convert to JSON Web Key format

var certificateRequest = new CertificateRequest("CN=access token key", rawRsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
var x509 = certificateRequest.CreateSelfSigned(now, now.AddDays(365));

// var x509 = new X509Certificate2(rsaPublicKeyBytes);
// var x509 = new X509Certificate2(Convert.FromBase64String("MIIDBTCCAe2gAwIBAgIQGQ6YG6NleJxJGDRAwAd/ZTANBgkqhkiG9w0BAQsFADAtMSswKQYDVQQDEyJhY2NvdW50cy5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MB4XDTIyMTAwMjE4MDY0OVoXDTI3MTAwMjE4MDY0OVowLTErMCkGA1UEAxMiYWNjb3VudHMuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALSS+lq9iVLMS8jXsz0IdSes5+sEqAwIYEWEg5GjLhB8u+VYpIgfMINuVrkfeoHTKaKJHZUb4e0p0b7Y0DfW+ZuMyQjKUkXCeQ7l5eJnHewoN2adQufiZjKvCe5uzkvR6VEGwNcobQh6j+1wOFJ0CNvCfk5xogGt74jy5atOutwquoUMO42KOcjY3SXFefhUvsTVe1B0eMwDEa7jFB8bXtSGSc2yZsYyqBIycA07XHeg5CN8q5JmLfBnUJrtGAR0yUmYs/jNdAmNy27y83/rWwTSkP4H5xhihezL0QpjwP2BfwD8p6yBu6eLzw0V4aRt/wiLd9ezcrxqCMIr9ALfN5ECAwEAAaMhMB8wHQYDVR0OBBYEFJcSH+6Eaqucndn9DDu7Pym7OA8rMA0GCSqGSIb3DQEBCwUAA4IBAQADKkY0PIyslgWGmRDKpp/5PqzzM9+TNDhXzk6pw8aESWoLPJo90RgTJVf8uIj3YSic89m4ftZdmGFXwHcFC91aFe3PiDgCiteDkeH8KrrpZSve1pcM4SNjxwwmIKlJdrbcaJfWRsSoGFjzbFgOecISiVaJ9ZWpb89/+BeAz1Zpmu8DSyY22dG/K6ZDx5qNFg8pehdOUYY24oMamd4J2u2lUgkCKGBZMQgBZFwk+q7H86B/byGuTDEizLjGPTY/sMms1FAX55xBydxrADAer/pKrOF1v7Dq9C1Z9QVcm5D9G4DcenyWUdMyK43NXbVQLPxLOng51KO9icp2j4U7pwHP"));
var x509SecurityKey = new X509SecurityKey(x509);
var jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(x509SecurityKey);
var jsonWebKeyRsa = JsonWebKeyConverter.ConvertFromRSASecurityKey(accessSecurityKey);
// Console.WriteLine("jswk '{0}'", jsonWebKey);
// Console.WriteLine("jswk '{0}'; '{1}'", jsonWebKeyRsa.N, jsonWebKeyRsa.E);

var jwks = new Dictionary<string, object>
{
    { "keys", new List<object>
    {
        new Dictionary<string, object>
        {
            { "kty", "RSA" },
            { "use", "sig" },
            { "kid", accessSecurityKey.KeyId },
            { "issuer", Options.AccessTokenIssuer },
            { "n", jsonWebKeyRsa.N },
            { "e", jsonWebKeyRsa.E },
            { "x5c", jsonWebKey.X5c }
        }
    } }
};

var json = JsonSerializer.Serialize<Dictionary<string, object>>(jwks, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
// Console.WriteLine(json);
File.WriteAllText(accesJwksPath, json, Encoding.UTF8);
Console.WriteLine("Access token JWKS written to: {0}", accesJwksPath);

// Double check validation with the X509 certificate
var readJwks = new JsonWebKeySet(json);
// Console.WriteLine(readJwks.Keys.First().X5c.First());
validationResult = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
{
    ValidIssuer = Options.AccessTokenIssuer,
    ValidAudiences = new string[] { Options.Audience },
    // IssuerSigningKey = x509SecurityKey
    IssuerSigningKey = new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(jsonWebKey.X5c.First())))
});

if (!validationResult.IsValid) throw new Exception("Access token verification failed with Certificate read from JWKS file");
// Console.WriteLine("Access token validation w/ CERTIFICATE {0}", validationResult.IsValid);
