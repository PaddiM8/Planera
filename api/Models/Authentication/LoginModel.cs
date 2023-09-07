using System.ComponentModel.DataAnnotations;

namespace Planera.Models.Authentication;

public class LoginModel
{
    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string Username { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string Password { get; init; }
}