namespace AuthServer.Api.OAuth.Entities;

public sealed class AuthorizationCode
{
    public int Id { get; set; }
    public required string CodeHash { get; set; }
    public int OAuthClientId { get; set; }
    public int UserId { get; set; }
    public required string RedirectUri { get; set; }
    public required string Scope { get; set; }
    public required string CodeChallenge { get; set; }
    public required string CodeChallengeMethod { get; set; }
    public string? Nonce { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public OAuthClient OAuthClient { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
}