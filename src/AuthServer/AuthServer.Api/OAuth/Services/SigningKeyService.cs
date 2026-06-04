using System.Security.Cryptography;
using AuthServer.Api.Identity;
using AuthServer.Api.OAuth.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Api.OAuth.Services;

public sealed class SigningKeyService
{
    private readonly AuthDbContext _dbContext;

    public SigningKeyService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SigningCredentials> GetSigningCredentialsAsync()
    {
        var key = await GetOrCreateActiveKeyAsync();

        var rsa = RSA.Create();
        rsa.ImportFromPem(key.PrivateKeyPem);

        return new SigningCredentials(
            new RsaSecurityKey(rsa) { KeyId = key.KeyId },
            SecurityAlgorithms.RsaSha256
        );
    }

    public async  Task<object> GetPublicJsonWebKeyAsync()
    {
        var key = await GetOrCreateActiveKeyAsync();

        var rsa = RSA.Create();
        rsa.ImportFromPem(key.PublicKeyPem);

        var parameters = rsa.ExportParameters(includePrivateParameters: false);

        return new
        {
            kty = "RSA",
            use = "sig",
            kid = key.KeyId,
            alg = key.Algorithm,
            n = WebEncoders.Base64UrlEncode(parameters.Modulus!),
            e = WebEncoders.Base64UrlEncode(parameters.Exponent!)
        };
    }

    private async Task<SigningKey> GetOrCreateActiveKeyAsync()
    {
        var existingKey = await _dbContext.SigningKeys
            .SingleOrDefaultAsync(key => key.IsActive && key.RetiredAt == null);

        if (existingKey is not null)
        {
            return existingKey;
        }

        using var rsa = RSA.Create(2048);

        var signingKey = new SigningKey
        {
            KeyId = Guid.NewGuid().ToString("N"),
            Algorithm = SecurityAlgorithms.RsaSha256,
            PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(),
            PublicKeyPem = rsa.ExportRSAPublicKeyPem(),
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        _dbContext.SigningKeys.Add(signingKey);
        await _dbContext.SaveChangesAsync();

        return signingKey;
    }
}