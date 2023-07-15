using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class RegisterModel
{
    [Required]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(20, ErrorMessage = ErrorStrings.StringLength)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(60, ErrorMessage = ErrorStrings.StringLength)]
    public required string Email { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(512, ErrorMessage = ErrorStrings.StringLength)]
    public required string Password { get; init; }
}