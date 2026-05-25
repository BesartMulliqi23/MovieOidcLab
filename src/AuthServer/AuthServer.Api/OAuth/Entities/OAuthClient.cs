namespace AuthServer.Api.OAuth.Entities;

public sealed class OAuthClient
{
    public int Id { get; set; }
    public required string ClientId { get; set; } 
    public required string ClientName { get; set; }
    public required string ClientType { get; set; }
    public bool RequirePkce { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<OAuthClientRedirectUri> RedirectUris { get; set; } = [];
    public List<OAuthClientScope> AllowedScopes { get; set; } = [];  
}