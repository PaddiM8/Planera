using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ErrorOr;
using Planera.Data;
using Planera.Models;

namespace Planera.Services;

public class AuthenticationService
{
    private readonly SymmetricSecurityKey _secretKey;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationService(IConfiguration configuration,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? string.Empty)
        );
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ErrorOr<AuthenticationResult>> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username) ??
                   await _userManager.FindByEmailAsync(model.Username);

        if (user == null)
        {
            return Error.NotFound(
                "Username.NotFound",
                "A user with the given username/email was not found"
            );
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            isPersistent: true,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            var token = GenerateToken(user.Id, user.UserName, user.Email);

            return new AuthenticationResult(token, user.UserName, user.Email);
        }

        if (result.IsLockedOut)
            Error.Failure("LockedOut", "Locked out.");

        return Error.Failure("NotAllowed", "Could not login.");
    }

    public async Task<ErrorOr<AuthenticationResult>> RegisterAsync(RegisterModel model)
    {
        var user = new User { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var token = GenerateToken(user.Id, model.Username, model.Username);

            return new AuthenticationResult(token, user.UserName, user.Email);
        }

        return result.Errors
            .Select(x => Error.Failure(x.Code, x.Description))
            .ToList();
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    private string GenerateToken(string id, string username, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", id),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "User"),
            }),
            Expires = DateTime.UtcNow.AddDays(90),
            SigningCredentials = new SigningCredentials(
                _secretKey,
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}