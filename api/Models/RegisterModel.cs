using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class RegisterModel
{
    [Required]
    [MinLength(2)]
    [StringLength(20)]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(60)]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    [StringLength(512)]
    public required string Password { get; init; }
}