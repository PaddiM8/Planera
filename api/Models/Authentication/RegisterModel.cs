using System.ComponentModel.DataAnnotations;

namespace Planera.Models.Authentication;

public class RegisterModel
{
    [Required(ErrorMessage = ErrorStrings.Required)]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(20, ErrorMessage = ErrorStrings.StringLength)]
    [RegularExpression(@"[\w-.]+")]
    public required string Username { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [EmailAddress(ErrorMessage = ErrorStrings.Email)]
    [StringLength(256, ErrorMessage = ErrorStrings.StringLength)]
    public required string Email { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string Password { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
    public required string ConfirmedPassword { get; init; }
}