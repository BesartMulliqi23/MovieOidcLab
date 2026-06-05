namespace Movies.Api.Entities;

public sealed class Movie
{
    public int Id { get; set; }
    public required string OwnerUserId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public DateOnly? WatchedAt { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}