using System;
using System.Collections;
using System.Collections.Generic;

public record class TokenOptions
{
    // SUbject field, i.e, 'sub' claim
    public string UserIdentity { get; set; } = "fake-user-identity";
    public string UserFullName { get; set; } = "Fake User";
    public string UserGivenName { get; set; } = "Fake";
    public string UserFamilyName { get; set; } = "User";
    public string UserEmail { get; set; } = "fake.user@example.com";
    public List<string> Roles { get; set; } = new List<string>() { "admin" };
    public string RolesClaimName { get; set; } = "roles";
    public bool IncludeRolesInUserData { get; set; } = false;
    public string ClaimsDataIssuer { get; set; } = "http://localhost/claimsdata";
    public string AccessTokenIssuer { get; set; } = "http://localhost/tokens/00000000-0000-0000-0000-000000000002";
    // 'aud' claim
    public string Audience { get; set; } = "fake-audience";
    // 'appid' or 'apz' claim
    public string ApplicationId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001").ToString();
    public bool UseApzForAppId { get; set; } = false;
    // 'tid' Active Directory claim
    public string TenantId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000002").ToString();
    public bool UseTenantId { get; set; } = true;
    public TokenExtraClaims ClaimsTokenExtra { get; set; } = new TokenExtraClaims();
    public TokenExtraClaims AccessTokenExtra { get; set; } = new TokenExtraClaims();
}

public record class TokenExtraClaims
{
    public Dictionary<string, object> Header { get; set; } = new();
    public Dictionary<string, object> Payload { get; set; } = new();
}