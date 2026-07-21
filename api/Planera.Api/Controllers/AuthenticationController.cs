using ErrorOr;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Planera.Api.Models.Authentication;
using Planera.Api.Models.User;
using Planera.Api.Services;
using Planera.Api.Extensions;
using Planera.Api.Models;

namespace Planera.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController(
    PlaneraAuthenticationService authenticationService,
    IOptions<OidcOptions> oidcOptions,
    IConfiguration configuration
)
    : ControllerBase
{
    private readonly PlaneraAuthenticationService _authenticationService = authenticationService;
    private readonly IOptions<OidcOptions> _oidcOptions = oidcOptions;
    private readonly IConfiguration _configuration = configuration;
    
    [AllowAnonymous]
    [HttpGet("info")]
    [ProducesResponseType(typeof(AuthenticationInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInfo()
    {
        var info = new AuthenticationInfo
        {
            PasswordAuthenticationDisabled = _configuration.GetValue<bool>("DisablePasswordAuthentication"),
            RegistrationDisabled = _configuration.GetValue<bool>("DisableRegistration"),
        };

        if (_oidcOptions.Value.ProviderId != null)
        {
            info.Oidc = new OidcAuthentiationInfo
            {
                ProviderId = _oidcOptions.Value.ProviderId,
                ProviderName = _oidcOptions.Value.ProviderName!,
                ProviderIconUrl = _oidcOptions.Value.ProviderIconUrl,
                RequireVerifiedEmail = _oidcOptions.Value.RequireVerifiedEmail,
            };
        }

        return Ok(info);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (_configuration.GetValue<bool>("DisablePasswordAuthentication"))
            return Error.Conflict("Login", "Password authentication has been disabled.").ToActionResult();

        var result = await _authenticationService.LoginAsync(model.Username, model.Password);

        return result.ToActionResult();
    }
    
    [AllowAnonymous]
    [HttpGet("login/oidc")]
    [ProducesResponseType(typeof(ChallengeResult), StatusCodes.Status200OK)]
    public IActionResult LoginOidc()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/",
        };

        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (_configuration.GetValue<bool>("DisableRegistration"))
            return Error.Conflict("Registration", "Registration has been disabled.").ToActionResult();

        var result = await _authenticationService.RegisterAsync(model.Username, model.Email, model.Password);

        return result.ToActionResult();
    }

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
