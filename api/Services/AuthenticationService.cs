using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ErrorOr;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Models;

namespace Planera.Services;

public class AuthenticationService
{
    private readonly SymmetricSecurityKey _secretKey;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IMapper _mapper;

    public AuthenticationService(
        IConfiguration configuration,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IMapper mapper)
    {
        _secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(configuration["Jwt:Key"] ?? string.Empty)
        );
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
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
            var token = GenerateToken(user.Id, user.UserName!, user.Email!);

            return new AuthenticationResult(token, _mapper.Map<UserDto>(user));
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

            return new AuthenticationResult(token, _mapper.Map<UserDto>(user));
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

    public async Task<ErrorOr<Updated>> ForgotPassword(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return Error.NotFound("Username.NotFound", "A user with the given username was not found.");

        if (_configuration["Smtp:Host"] == null)
            return Error.Conflict("NotSupported", "The server is not equipped to send emails.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration["Smtp:Sender"]));
        email.To.Add(MailboxAddress.Parse(user.Email));
        email.Subject = "Password Reset";
        email.Body = new TextPart(TextFormat.Text)
        {
            Text = $"""
                Someone requested a password reset for your account.
                If you did this, you can reset your password here:
                {_configuration["FrontendUrl"]}/reset-password?user={user.Id}&token={HttpUtility.UrlEncode(resetToken)}

                If you did not expect this email, feel free to ignore it.

                - Planera
                """,
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _configuration["Smtp:Host"],
            _configuration.GetValue<int>("Smtp:Port"),
            SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(_configuration["Smtp:User"], _configuration["Smtp:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return new ErrorOr<Updated>();
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
}