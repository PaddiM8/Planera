using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class ChangePasswordModel
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string NewPassword { get; init; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
    public required string ConfirmedPassword { get; init; }
}