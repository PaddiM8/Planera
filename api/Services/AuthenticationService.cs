using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ErrorOr;
using MimeKit;
using MimeKit.Text;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Models;
using Planera.Models.Authentication;

namespace Planera.Services;

public class PlaneraAuthenticationService(
    IConfiguration configuration,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IMapper mapper,
    EmailService emailService
)
{
    private readonly SymmetricSecurityKey _secretKey = new(
        Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? string.Empty)
    );
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly IMapper _mapper = mapper;
    private readonly EmailService _emailService = emailService;

    public async Task<ErrorOr<AuthenticationResult>> LoginAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return Error.NotFound(
                "Username.NotFound",
                "A user with the given username/email was not found."
            );
        }

        if (_configuration.GetValue<bool>("EmailConfirmation") && !user.EmailConfirmed)
        {
            return Error.Conflict(
                "Email.NotConfirmed",
                "The email address of this user has not been confirmed."
            );
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: true,
            lockoutOnFailure: true
        );

        if (result.Succeeded)
        {
            var token = GenerateToken(user.Id, user.UserName!, user.Email!);

            return new AuthenticationResult(token, _mapper.Map<UserDto>(user));
        }

        if (result.IsLockedOut)
            Error.Failure("Username.LockedOut", "Locked out.");

        return Error.Failure("Password.Invalid", "Could not login. Is the password correct?");
    }

    public async Task<ErrorOr<AuthenticationResult?>> RegisterAsync(
        string username,
        string email,
        string password)
    {
        var userResult = await CreateUserAsync(
            username,
            email,
            password
        );
        if (userResult.IsError)
            return userResult.Errors;

        var user = userResult.Value;
        if (_configuration.GetValue<bool>("EmailConfirmation"))
        {
            await SendConfirmationEmailAsync(user);

            return ErrorOrFactory.From<AuthenticationResult?>(null);
        }

        var loginToken = GenerateToken(user.Id, username, email);

        return new AuthenticationResult(loginToken, _mapper.Map<UserDto>(user));
    }

    private async Task<ErrorOr<User>> CreateUserAsync(
        string username,
        string email,
        string password)
    {
        var user = new User { UserName = username, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
            return user;

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
                new Claim(ClaimTypes.NameIdentifier, username),
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

    public async Task<ErrorOr<Success>> ForgotPassword(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return Error.NotFound("Username.NotFound", "A user with the given username was not found.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var emailBody = new TextPart(TextFormat.Text)
        {
            Text = $"""
                Someone requested a password reset for your account.
                If you did this, you can reset your password here:
                {_configuration["FrontendUrl"]}/reset-password?user={user.Id}&token={HttpUtility.UrlEncode(resetToken)}

                If you did not expect this email, feel free to ignore it.

                - Planera
                """,
        };

        return await _emailService.SendAsync("Password Reset", emailBody, user.Email!);
    }

    public async Task<ErrorOr<Updated>> ResetPasswordAsync(
        string userId,
        string resetToken,
        string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        return !result.Succeeded
            ? Error.Failure("General.Failure", "Failed to reset password.")
            : new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Success>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        return !result.Succeeded
            ? Error.Failure("General.Failure", "Failed to confirm email.")
            : new ErrorOr<Success>();
    }

    public async Task<ErrorOr<Success>> SendConfirmationEmailAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return Error.NotFound("Username.NotFound", "A user with the given username was not found.");

        return await SendConfirmationEmailAsync(user);
    }

    public async Task<ErrorOr<Success>> SendConfirmationEmailAsync(User user)
    {
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var emailBody = new TextPart(TextFormat.Text)
        {
            Text = $"""
            Someone signed up to Planera using your email. If it was you, please confirm your email by pressing the link below:
            {_configuration["FrontendUrl"]}/confirm-email?user={user.Id}&token={HttpUtility.UrlEncode(confirmationToken)}

            - Planera
            """,
        };
        await _emailService.SendAsync("Email Confirmation", emailBody, user.Email!);

        return new ErrorOr<Success>();
    }
}
