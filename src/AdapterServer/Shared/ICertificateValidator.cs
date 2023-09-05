
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AdapterServer.Shared;

public interface ICertificateValidator
{
    /// <summary>
    /// The ICertificateValidator instance to delegate to.
    /// Useful for when more information is needed and/or delayed instantiation is required, e.g., via dependency injection.
    /// </summary>
    public static ICertificateValidator? Instance { get; set; } = null;

    /// <summary>
    /// Static certificate validation method, conforming to the RemoteCertificateValidationCallback delegate,
    /// to be provided to the configuration.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="certificate"></param>
    /// <param name="chain"></param>
    /// <param name="sslPolicyErrors"></param>
    /// <returns>true if the certificate is to be accepted/connection allowed, false otherwise.</returns>
    public static bool ValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (Instance is not null) return Instance.Validate(sender, certificate, chain, sslPolicyErrors);
        if (sslPolicyErrors == SslPolicyErrors.None) return true;
        return false;
    }

    public bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors);
}