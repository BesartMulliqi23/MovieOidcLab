using System.Security.Cryptography;
using System.Text;
using AuthServer.Api.Identity;
using AuthServer.Api.OAuth.Entities;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthServer.Api.OAuth.Services;

public sealed class AuthorizationCodeService
{
    private readonly AuthDbContext _dbContext;

    public AuthorizationCodeService(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateAsync(
        int oauthClientId,
        int userId,
        string redirectUri,
        IReadOnlyCollection<string> scopes,
        string codeChallenge,
        string codeChallengeMethod,
        string? nonce
    )
    {
        var code = GenerateCode();

        var authorizationCode = new AuthorizationCode
        {
            CodeHash = HashCode(code),
            OAuthClientId = oauthClientId,
            UserId = userId,
            RedirectUri = redirectUri,
            Scope = string.Join(' ', scopes),
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            Nonce = nonce,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        _dbContext.AuthorizationCodes.Add(authorizationCode);

        await _dbContext.SaveChangesAsync();

        return code;
    }

    public static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));

        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return WebEncoders.Base64UrlEncode(bytes);
    }
}