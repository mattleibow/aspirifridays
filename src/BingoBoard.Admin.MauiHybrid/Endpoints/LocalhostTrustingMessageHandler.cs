using System.Net.Security;

namespace BingoBoard.Admin.MauiHybrid.Endpoints;

/// <summary>
/// HTTP message handler that trusts localhost SSL certificates for development purposes.
/// This is useful when connecting to a local HTTPS development server with a self-signed certificate.
/// </summary>
public class LocalhostTrustingMessageHandler : HttpClientHandler
{
    public LocalhostTrustingMessageHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            // Trust all certificates when connecting to localhost for development
            if (message.RequestUri?.Host == "localhost" || message.RequestUri?.Host == "127.0.0.1")
            {
                return true;
            }

            // For non-localhost connections, use the default certificate validation
            return errors == SslPolicyErrors.None;
        };
    }
}
