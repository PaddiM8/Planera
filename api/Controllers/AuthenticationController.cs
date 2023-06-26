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
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        => (await _authenticationService.LoginAsync(model)).ToActionResult();

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        => (await _authenticationService.RegisterAsync(model)).ToActionResult();

    [HttpGet("logout")]
    public async Task LogoutAsync()
    {
        await _authenticationService.LogoutAsync();
    }
}