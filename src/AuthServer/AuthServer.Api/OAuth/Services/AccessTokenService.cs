using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthServer.Api.Identity;

namespace AuthServer.Api.OAuth.Services;

public sealed class AccessTokenService
{
    private readonly IConfiguration _configuration;
    private readonly SigningKeyService _signingKeyService;

    public AccessTokenService(IConfiguration configuration, SigningKeyService signingKeyService)
    {
        _configuration = configuration;
        _signingKeyService = signingKeyService;
    }

    public TokenResponse CreateAccessToken(ApplicationUser user, string clientId, string scope)
    {
        var issuer = _configuration["OAuth:Issuer"]!;
        var lifetimeMinutes = _configuration.GetValue<int>("OAuth:AccessTokenLifetimeMinutes", 15);
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("client_id", clientId),
            new("scope", scope)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: "movies-api", // hardcoded for now, but we may want to make this dynamic in the future
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: _signingKeyService.GetSigningCredentials()
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse(
            accessToken,
            "Bearer",
            lifetimeMinutes * 60, // convert to seconds
            scope
        );
    }
}