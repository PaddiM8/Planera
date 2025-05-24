using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Planera.Data;
using Planera.Services;

namespace Planera;

public class PersonalAccessTokenHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    PersonalAccessTokenService personalAccessTokenService,
    UserManager<User> userManager
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly PersonalAccessTokenService _personalAccessTokenService = personalAccessTokenService;
    private readonly UserManager<User> _userManager = userManager;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var value))
            return AuthenticateResult.NoResult();

        var authHeader = value.ToString();
        if (!authHeader.StartsWith("Pat ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Pat ".Length..].Trim();
        var userId = await _personalAccessTokenService.ValidateAsync(token);
        if (userId == null)
            return AuthenticateResult.Fail("Invalid PAT");

        var user = await _userManager.FindByIdAsync(userId);
        var claims = new[] {
            new Claim("Id", userId),
            new Claim(ClaimTypes.NameIdentifier, user?.UserName ?? string.Empty),
            new Claim(ClaimTypes.Name, user?.UserName ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
