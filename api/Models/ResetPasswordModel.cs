using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class ResetPasswordModel
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    public required string ResetToken { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string NewPassword { get; init; }

    [Compare(nameof(NewPassword))]
    public required string ConfirmedPassword { get; init; }
}