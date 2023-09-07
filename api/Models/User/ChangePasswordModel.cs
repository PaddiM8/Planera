using System.ComponentModel.DataAnnotations;

namespace Planera.Models.User;

public class ChangePasswordModel
{
    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string CurrentPassword { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string NewPassword { get; init; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
    public required string ConfirmedPassword { get; init; }
}