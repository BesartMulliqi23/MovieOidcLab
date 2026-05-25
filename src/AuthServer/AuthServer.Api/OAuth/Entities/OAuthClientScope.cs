namespace AuthServer.Api.OAuth.Entities;

public sealed class OAuthClientScope
{
    public int Id { get; set; }
    public int OAuthClientId { get; set; }
    public required string Scope { get; set; }
    public OAuthClient OAuthClient { get; set; } = null!;
}