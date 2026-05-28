using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthServer.Api.OAuth.Services;

public static class PkceService
{
    public static bool ValidateS256(string codeVerifier, string expectedCodeChallenge)
    {
        var actualCodeChallenge = CreateS256CodeChallenge(codeVerifier);

        var actualBytes = Encoding.ASCII.GetBytes(actualCodeChallenge);
        var expectedBytes = Encoding.ASCII.GetBytes(expectedCodeChallenge);

        return actualBytes.Length == expectedBytes.Length && 
            CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    private static string CreateS256CodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));

        return WebEncoders.Base64UrlEncode(bytes);
    }
}