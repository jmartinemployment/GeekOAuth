using System.Security.Cryptography.X509Certificates;

namespace GeekOAuth.Infrastructure;

public static class SigningCertificateLoader
{
    public static (X509Certificate2? Signing, X509Certificate2? Encryption) Load(IConfiguration config)
    {
        var signing = LoadCert(config["SIGNING_CERT_BASE64"], config["SIGNING_CERT_PASSWORD"]);
        var encryption = LoadCert(config["ENCRYPTION_CERT_BASE64"], config["ENCRYPTION_CERT_PASSWORD"]);
        return (signing, encryption);
    }

    private static X509Certificate2? LoadCert(string? base64, string? password)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;

        var bytes = Convert.FromBase64String(base64);
        return string.IsNullOrWhiteSpace(password)
            ? X509CertificateLoader.LoadPkcs12(bytes, null)
            : X509CertificateLoader.LoadPkcs12(bytes, password);
    }
}
