using AuthServer.Api.OAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Api.Identity;

public sealed class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    public DbSet<OAuthClient> OAuthClients => Set<OAuthClient>();
    public DbSet<OAuthClientRedirectUri> OAuthClientRedirectUris => Set<OAuthClientRedirectUri>();
    public DbSet<OAuthClientScope> OAuthClientScopes => Set<OAuthClientScope>();
    public DbSet<AuthorizationCode> AuthorizationCodes => Set<AuthorizationCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OAuthClient>(entity =>
        {
            entity.HasIndex(client => client.ClientId).IsUnique();

            entity.Property(client => client.ClientId).HasMaxLength(100).IsRequired();
            entity.Property(client => client.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(client => client.ClientType).HasMaxLength(50).IsRequired();

            entity.HasMany(client => client.RedirectUris)
                .WithOne(uri => uri.OAuthClient)
                .HasForeignKey(uri => uri.OAuthClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(client => client.AllowedScopes)
                .WithOne(scope => scope.OAuthClient)
                .HasForeignKey(scope => scope.OAuthClientId)
                .OnDelete(DeleteBehavior.Cascade);    
        });

        builder.Entity<OAuthClientRedirectUri>(entity =>
        {
            entity.Property(uri => uri.RedirectUri).HasMaxLength(500).IsRequired();

            entity.HasIndex(uri => new
            {
                uri.OAuthClientId,
                uri.RedirectUri
            }).IsUnique();
        });

        builder.Entity<OAuthClientScope>(entity =>
        {
            entity.Property(scope => scope.Scope).HasMaxLength(100).IsRequired();

            entity.HasIndex(scope => new
            {
                scope.OAuthClientId,
                scope.Scope
            }).IsUnique();
        });

        builder.Entity<OAuthClient>().HasData(new OAuthClient
        {
            Id = 1,
            ClientId = "movies-spa",
            ClientName = "Movies SPA",
            ClientType = "public",
            RequirePkce = true,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2026, 5, 25, 0, 0, 0, TimeSpan.Zero)
        });

        builder.Entity<OAuthClientRedirectUri>().HasData(new OAuthClientRedirectUri
        {
            Id = 1,
            OAuthClientId = 1,
            RedirectUri = "http://localhost:5173/callback"
        });

        builder.Entity<OAuthClientScope>().HasData(
            new OAuthClientScope { Id = 1, OAuthClientId = 1, Scope = "openid" },
            new OAuthClientScope { Id = 2, OAuthClientId = 1, Scope = "profile" },
            new OAuthClientScope { Id = 3, OAuthClientId = 1, Scope = "offline_access" },
            new OAuthClientScope { Id = 4, OAuthClientId = 1, Scope = "movies.read" },
            new OAuthClientScope { Id = 5, OAuthClientId = 1, Scope = "movies.write" }
        );

        builder.Entity<AuthorizationCode>(entity =>
        {
            entity.HasIndex(code => code.CodeHash).IsUnique();

            entity.Property(code => code.CodeHash).HasMaxLength(100).IsRequired();
            entity.Property(code => code.RedirectUri).HasMaxLength(500).IsRequired();
            entity.Property(code => code.Scope).HasMaxLength(500).IsRequired();
            entity.Property(code => code.CodeChallenge).HasMaxLength(200).IsRequired();
            entity.Property(code => code.CodeChallengeMethod).HasMaxLength(20).IsRequired();
            entity.Property(code => code.Nonce).HasMaxLength(200);

            entity.HasOne(code => code.OAuthClient)
                .WithMany()
                .HasForeignKey(code => code.OAuthClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(token => token.TokenHash).IsUnique();

            entity.Property(token => token.TokenHash).HasMaxLength(100).IsRequired();
            entity.Property(token => token.RotationFamilyId).HasMaxLength(100).IsRequired();
            entity.Property(token => token.Scope).HasMaxLength(500).IsRequired();

            entity.HasOne(token => token.OAuthClient)
                .WithMany()
                .HasForeignKey(token => token.OAuthClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}