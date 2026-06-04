namespace AuthServer.Api.OAuth.Entities;

public sealed class SigningKey
{
    public int Id { get; set; }
    public required string KeyId { get; set; }
    public required string Algorithm { get; set; }
    public required string PrivateKeyPem { get; set; }
    public required string PublicKeyPem { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RetiredAt { get; set; }
    public bool IsActive { get; set; } = true;
}