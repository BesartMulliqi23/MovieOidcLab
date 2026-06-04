using System.Threading.Tasks;
using AuthServer.Api.OAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
public sealed class DiscoveryController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SigningKeyService _signingKeyService;

    public DiscoveryController(IConfiguration configuration, SigningKeyService signingKeyService)
    {
        _configuration = configuration;
        _signingKeyService = signingKeyService;
    }

    [HttpGet(".well-known/openid-configuration")]
    public IActionResult OpenIdConfiguration()
    {
        var issuer = _configuration["OAuth:Issuer"];

        return Ok(new
        {
            issuer,
            authorization_endpoint = $"{issuer}/connect/authorize",
            token_endpoint = $"{issuer}/connect/token",
            jwks_uri = $"{issuer}/.well-known/jwks.json",

            response_types_supported = new[] { "code" },
            grant_types_supported = new[] { "authorization_code", "refresh_token" },
            scopes_supported = new[] { "openid", "profile", "offline_access", "movies.read", "movies.write" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" },
            token_endpoint_auth_methods_supported = new[] { "none" },
            code_challenge_methods_supported = new[] { "S256" },
            claims_supported = new[] { "sub", "email", "name", "nonce" }
        });
    }

    [HttpGet(".well-known/jwks.json")]
    public async Task<IActionResult> JsonWebKeySet()
    {
        return Ok(new
        {
            keys = new[] { await _signingKeyService.GetPublicJsonWebKeyAsync() }
        });
    }
}