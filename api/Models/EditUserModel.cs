using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class EditUserModel
{
    [Required]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(20, ErrorMessage = ErrorStrings.StringLength)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(60, ErrorMessage = ErrorStrings.StringLength)]

    public required string Email { get; init; }
}