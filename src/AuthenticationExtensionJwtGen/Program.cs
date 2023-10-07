using System.Buffers.Text;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

// Create new public key using same algorithm as AWS Load Balancer (ES256)
var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var publicKeyBytes = key.ExportSubjectPublicKeyInfo();
var encoded = PemEncoding.Write("EC PUBLIC KEY", publicKeyBytes);
File.WriteAllText("pub_key.txt", new string(encoded));
Console.WriteLine("{0}\n", new string(encoded));

// Generate a new JWT
var tokenHandler = new JsonWebTokenHandler();

var now = DateTime.UtcNow;

var identity = new ClaimsIdentity(new Claim[]{
    new Claim("sub", "fakeUser"),
    new Claim(ClaimTypes.Name, "Fake User"),
    new Claim(ClaimTypes.Role, "Fake Role")
});

string token = tokenHandler.CreateToken(new SecurityTokenDescriptor
{
    Issuer = "TBD",
    Audience = "TBD",
    NotBefore = now,
    Expires = now.AddDays(365),
    IssuedAt = now,
    Subject = identity,
    SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(key), SecurityAlgorithms.EcdsaSha256)
});

File.WriteAllText("jwt.txt", token);

var jwtHeaders = tokenHandler.ReadJsonWebToken(token);

var decodedJwtHeaders = Base64UrlEncoder.Decode(jwtHeaders.EncodedHeader);
Console.WriteLine("Decoded headers {0}", decodedJwtHeaders);

var decodedPayload = Base64UrlEncoder.Decode(jwtHeaders.EncodedPayload);
Console.WriteLine("Decoded headers {0}", decodedPayload);
