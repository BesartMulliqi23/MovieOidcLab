using System.Security.Cryptography;
using System.Text;
using AuthServer.Api.Identity;
using AuthServer.Api.OAuth.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Api.OAuth.Services;

public sealed class RefreshTokenService
{
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(AuthDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<string> CreateAsync(int oauthClientId, int userId, string scope, string? rotationFamilyId = null)
    {
        var refreshToken = GenerateToken();
        var lifetimeDays = _configuration.GetValue<int>("OAuth:RefreshTokenLifetimeDays");

        var entity = new RefreshToken
        {
            TokenHash = Hash(refreshToken),
            RotationFamilyId = rotationFamilyId ?? Guid.NewGuid().ToString("N"),
            OAuthClientId = oauthClientId,
            UserId = userId,
            Scope = scope,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(lifetimeDays)
        };

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshTokenValidationResult> ValidateForRotationAsync(string refreshToken, int oauthClientId)
    {
        var tokenHash = Hash(refreshToken);

        var token = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (token is null)
        {
            return RefreshTokenValidationResult.Failure("invalid_grant", "The refresh token is invalid.");
        }

        if (token.OAuthClientId != oauthClientId)
        {
            return RefreshTokenValidationResult.Failure("invalid_grant", "The refresh token was not issued to this client.");
        }

        if (token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenValidationResult.Failure("invalid_grant", "The refresh token has expired.");
        }

        if (token.RevokedAt is not null)
        {
            token.ReuseDetectedAt = DateTimeOffset.UtcNow;

            var familyTokens = await _dbContext.RefreshTokens
                .Where(candidate => candidate.RotationFamilyId == token.RotationFamilyId)
                .ToListAsync();

            foreach (var familyToken in familyTokens.Where(candidate => candidate.RevokedAt is null))
            {
                familyToken.RevokedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return RefreshTokenValidationResult.Failure(
                "invalid_grant", 
                "Refresh token reuse was detected. The token family has been revoked."
            );
        }

        return RefreshTokenValidationResult.Success(token);
    }

    public async Task<string> RotateAsync(RefreshToken oldToken)
    {
        var newRefreshToken = await CreateAsync(
            oldToken.OAuthClientId,
            oldToken.UserId,
            oldToken.Scope,
            oldToken.RotationFamilyId
        );

        var newTokenHash = Hash(newRefreshToken);

        var newToken = await _dbContext.RefreshTokens
            .SingleAsync(token => token.TokenHash == newTokenHash);

        oldToken.RevokedAt = DateTimeOffset.UtcNow;
        oldToken.ReplacedByRefreshTokenId = newToken.Id;

        await _dbContext.SaveChangesAsync();

        return newRefreshToken;
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(bytes);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}

public sealed class RefreshTokenValidationResult
{
    private RefreshTokenValidationResult(bool succeeded, RefreshToken? refreshToken, string? error, string? errorDescription)
    {
        Succeeded = succeeded;
        RefreshToken = refreshToken;
        Error = error;
        ErrorDescription = errorDescription;
    }

    public bool Succeeded { get; }
    public RefreshToken? RefreshToken { get; }
    public string? Error { get; }
    public string? ErrorDescription { get; }

    public static RefreshTokenValidationResult Success(RefreshToken refreshToken)
    {
        return new RefreshTokenValidationResult(true, refreshToken, null, null);
    }

    public static RefreshTokenValidationResult Failure(string error, string errorDescription)
    {
        return new RefreshTokenValidationResult(false, null, error, errorDescription);
    }
}