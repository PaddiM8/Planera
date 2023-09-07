using System.ComponentModel.DataAnnotations;

namespace Planera.Models.User;

public class EditUserModel
{
    [Required]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(20, ErrorMessage = ErrorStrings.StringLength)]
    [RegularExpression(@"[\w-.]+")]
    public required string Username { get; init; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(256, ErrorMessage = ErrorStrings.StringLength)]

    public required string Email { get; init; }

    public string? Avatar { get; init; }
}