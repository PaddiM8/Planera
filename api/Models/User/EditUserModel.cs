using System.ComponentModel.DataAnnotations;
using Planera.Data;

namespace Planera.Models.User;

public class EditUserModel
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

    public string? Avatar { get; init; }

    public InterfaceTheme? Theme { get; init; }
}