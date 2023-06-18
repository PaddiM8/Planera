using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class LoginModel
{
    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}