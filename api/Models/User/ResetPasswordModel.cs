using System.ComponentModel.DataAnnotations;

namespace Planera.Models.User;

public class ResetPasswordModel
{
    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string UserId { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string ResetToken { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string NewPassword { get; init; }

    [Compare(nameof(NewPassword), ErrorMessage = ErrorStrings.PasswordsDoNotMatch)]
    public required string ConfirmedPassword { get; init; }
}