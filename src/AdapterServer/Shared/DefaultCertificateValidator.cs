using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Isbm2Client.Model;
using Microsoft.Extensions.Options;

namespace AdapterServer.Shared;

/// <summary>
/// The default certificate validator used by the ISBM Rest Client.
/// Fails on missing certificate or name mismatch.
/// Development mode allows self-signed certificates.
/// TODO: Notifications will be raised on failures.
/// TODO: Certificate pinning will be applied.
/// </summary>
public  class DefaultCertificateValidator : ICertificateValidator
{
    private readonly ILogger<ICertificateValidator> _logger;
    private readonly string _allowedHost;

    public DefaultCertificateValidator(IOptions<ClientConfig> clientConfig, ILogger<ICertificateValidator> logger)
    {
        _allowedHost = new Uri(clientConfig.Value.EndPoint).Host;
        _logger = logger;
        ICertificateValidator.Instance = this;
    }

    public bool Validate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None) return true;
        if ((sslPolicyErrors & (SslPolicyErrors.RemoteCertificateNotAvailable | SslPolicyErrors.RemoteCertificateNameMismatch)) != SslPolicyErrors.None)
        {
            _logger.LogWarning("ISBM SSL Certificate is either missing or has a name mismatch: {Reason}", sslPolicyErrors);
            return false;
        }

        // sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors
        _logger.LogWarning("ISBM SSL Certificate has invalid chain (possibly self-signed)", sslPolicyErrors);

        var request = sender as HttpRequestMessage;
        if (request is null || request is not null && request.RequestUri?.Host != _allowedHost)
        {
            _logger.LogWarning("Request to {Host} does not match expected {Hostname}", request?.RequestUri?.Host, _allowedHost);
        }

#if DEBUG
        return true;
#else
        return false;
#endif
    }
}