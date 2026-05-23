using AuthServer.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<CurrentUserResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => new
            {
                e.Code,
                e.Description
            }));
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new CurrentUserResponse(user.Id, user.Email, user.DisplayName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null) return Unauthorized(); 

        var result = await _signInManager.PasswordSignInAsync(
            user, 
            request.Password, 
            isPersistent: request.RememberMe, 
            lockoutOnFailure: true);
        
        if (!result.Succeeded) return Unauthorized();

        return Ok(new CurrentUserResponse(
            user.Id,
            user.Email!,
            user.DisplayName
        ));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null) return Unauthorized();

        return Ok(new CurrentUserResponse(
            user.Id,
            user.Email!,
            user.DisplayName
        ));
    }
}

public sealed record RegisterRequest(string Email, string Password, string? DisplayName);

public sealed record LoginRequest(string Email, string Password, bool RememberMe);

public sealed record CurrentUserResponse(int Id, string Email, string? DisplayName);