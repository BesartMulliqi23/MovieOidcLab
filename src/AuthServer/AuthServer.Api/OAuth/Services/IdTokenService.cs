using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthServer.Api.Identity;

namespace AuthServer.Api.OAuth.Services;

public sealed class IdTokenService
{
    private readonly IConfiguration _configuration;
    private readonly SigningKeyService _signingKeyService;

    public IdTokenService(IConfiguration configuration, SigningKeyService signingKeyService)
    {
        _configuration = configuration;
        _signingKeyService = signingKeyService;
    }

    public string CreateIdToken(ApplicationUser user, string clientId, string? nonce)
    {
        var issuer = _configuration["OAuth:Issuer"]!;
        var lifetimeMinutes = _configuration.GetValue<int>("OAuth:IdTokenLifetimeMinutes");

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(lifetimeMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.DisplayName));
        }

        if (!string.IsNullOrWhiteSpace(nonce))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Nonce, nonce));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: clientId,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingKeyService.GetSigningCredentials()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}