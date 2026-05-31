// For now, the key is still generated on app startup. Later we’ll persist signing keys so tokens survive app restarts

using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Api.OAuth.Services;

public sealed class SigningKeyService
{
    private readonly RsaSecurityKey _key;

    public SigningKeyService()
    {
        var rsa = RSA.Create(2048);

        _key = new RsaSecurityKey(rsa)
        {
            KeyId = Guid.NewGuid().ToString("N")
        };
    }

    public SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
    }

    public object GetPublicJsonWebKey()
    {
        var parameters = _key.Rsa!.ExportParameters(includePrivateParameters: false);

        return new
        {
            kty = "RSA",
            use = "sig",
            kid = _key.KeyId,
            alg = "RS256",
            n = WebEncoders.Base64UrlEncode(parameters.Modulus!),
            e = WebEncoders.Base64UrlEncode(parameters.Exponent!)
        };
    }
}