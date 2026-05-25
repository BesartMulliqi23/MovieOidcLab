namespace AuthServer.Api.OAuth.Entities;

public sealed class OAuthClientRedirectUri
{
    public int Id { get; set; }
    public int OAuthClientId { get; set; }
    public required string RedirectUri { get; set; }
    public OAuthClient OAuthClient { get; set; } = null!;
}