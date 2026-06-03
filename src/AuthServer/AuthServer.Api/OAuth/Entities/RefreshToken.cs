namespace AuthServer.Api.OAuth.Entities;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public required string RotationFamilyId { get; set; }

    public int OAuthClientId { get; set; }
    public int UserId { get; set; }

    public required string Scope { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } 

    public DateTimeOffset? RevokedAt { get; set; } 
    public int? ReplacedByRefreshTokenId { get; set; }
    public DateTimeOffset? ReuseDetectedAt { get; set; }

    public OAuthClient OAuthClient { get; set; } = null!;
}