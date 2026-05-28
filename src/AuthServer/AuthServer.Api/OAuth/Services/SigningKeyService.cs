// This is temporary dev behavior. Later we will persist signing keys and expose the public key through JWKS.

using System.Security.Cryptography;
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
}