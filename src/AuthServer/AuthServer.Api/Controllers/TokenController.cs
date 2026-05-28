using AuthServer.Api.OAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("connect")]
public sealed class TokenController : ControllerBase
{
    private readonly TokenExchangeService _tokenExchangeService;

    public TokenController(TokenExchangeService tokenExchangeService)
    {
        _tokenExchangeService = tokenExchangeService;
    }

    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request) 
    {
        var result = await _tokenExchangeService.ExchangeAuthorizationCodeAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                error = result.Error,
                error_description = result.ErrorDescription
            });
        }

        return Ok(result.TokenResponse);
    }
}