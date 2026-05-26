using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace GeekOAuth.IntegrationTests.Helpers;

internal static class PkceHelper
{
    public static (string Verifier, string Challenge) Generate()
    {
        var verifierBytes = RandomNumberGenerator.GetBytes(32);
        var verifier = WebEncoders.Base64UrlEncode(verifierBytes);
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = WebEncoders.Base64UrlEncode(challengeBytes);
        return (verifier, challenge);
    }
}
