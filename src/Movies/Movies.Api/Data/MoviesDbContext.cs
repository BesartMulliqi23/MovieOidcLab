using Microsoft.EntityFrameworkCore;
using Movies.Api.Entities;

namespace Movies.Api.Data;

public sealed class MoviesDbContext : DbContext
{
    public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) {}

    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Movie>(entity =>
        {
            entity.Property(movie => movie.OwnerUserId).HasMaxLength(100).IsRequired();
            entity.Property(movie => movie.Title).HasMaxLength(200).IsRequired();
            entity.Property(movie => movie.Description).HasMaxLength(1000);
            entity.Property(movie => movie.Comment).HasMaxLength(2000);

            entity.HasIndex(movie => movie.OwnerUserId);
        });
    }
}