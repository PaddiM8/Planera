using System.ComponentModel.DataAnnotations;

namespace Planera.Api.Models.User;

public class ChangePasswordModel
{
    public string? CurrentPassword { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string NewPassword { get; init; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
    public required string ConfirmedPassword { get; init; }
}