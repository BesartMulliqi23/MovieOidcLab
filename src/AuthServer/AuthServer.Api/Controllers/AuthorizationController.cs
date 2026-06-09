using System.Security.Claims;
using AuthServer.Api.OAuth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("connect")]
public sealed class AuthorizationController : ControllerBase
{
    private readonly AuthorizeRequestValidator _validator;
    private readonly AuthorizationCodeService _authorizationCodeService;

    public AuthorizationController(AuthorizeRequestValidator validator, AuthorizationCodeService authorizationCodeService)
    {
        _validator = validator;
        _authorizationCodeService = authorizationCodeService;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizeRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.Succeeded)
        {
            return BadRequest(new
            {
                error = validationResult.Error,
                error_description = validationResult.ErrorDescription,
                state = request.State
            });
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            var authUiBaseUrl = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Frontend:AuthUiBaseUrl"];

            var returnUrl = 
                $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}{Request.QueryString}";

            var loginUrl = QueryHelpers.AddQueryString(
                $"{authUiBaseUrl}/login",
                "returnUrl",
                returnUrl
            );

            return Redirect(loginUrl);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                error = "invalid_user",
                error_description = "The authenticated user id is invalid."
            });
        }

        var code = await _authorizationCodeService.CreateAsync(
            validationResult.Client!.Id,
            userId,
            request.RedirectUri!,
            validationResult.RequestedScopes,
            request.CodeChallenge!,
            request.CodeChallengeMethod!,
            request.Nonce
        );

        var redirectUri = QueryHelpers.AddQueryString(
            request.RedirectUri!,
            new Dictionary<string, string?>
            {
                ["code"] = code,
                ["state"] = request.State
            }
        );

        return Redirect(redirectUri);
    }
}