using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planera.Extensions;
using Planera.Models;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;

    public AuthenticationController(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
        => (await _authenticationService.LoginAsync(model)).ToActionResult();

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
        => (await _authenticationService.RegisterAsync(model)).ToActionResult();

    [HttpGet("logout")]
    public async Task Logout()
        => await _authenticationService.LogoutAsync();

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string username)
    {
        var result = await _authenticationService.ForgotPassword(username);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var result = await _authenticationService.ResetPasswordAsync(
            model.UserId,
            model.ResetToken,
            model.NewPassword
        );

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authenticationService.ConfirmEmailAsync(userId, token);

        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("send-confirmation-email")]
    public async Task<IActionResult> SendConfirmationEmail(string username)
    {
        var result = await _authenticationService.SendConfirmationEmailAsync(username);

        return result.ToActionResult();
    }
}