using AuthServer.Api.OAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("connect")]
public sealed class AuthorizationController : ControllerBase
{
    private readonly AuthorizeRequestValidator _validator;

    public AuthorizationController(AuthorizeRequestValidator validator)
    {
        _validator = validator;
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
            return Unauthorized(new
            {
                error = "login_required",
                error_description = "The user must be logged in before authorization can continue.",
                client_id = validationResult.Client!.ClientId,
                redirect_uri = request.RedirectUri,
                scope = validationResult.RequestedScopes,
                state = request.State
            });
        }

        return Ok(new
        {
            message = "Authorization request is valid and the user is authenticated.",
            client_id = validationResult.Client!.ClientId,
            client_name = validationResult.Client.ClientName,
            redirect_uri = request.RedirectUri,
            scope = validationResult.RequestedScopes,
            state = request.State,
            nonce = request.Nonce,
            code_challenge = request.CodeChallenge,
            code_challenge_method = request.CodeChallengeMethod
        });
    }
}