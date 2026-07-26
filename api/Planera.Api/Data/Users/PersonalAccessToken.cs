using System.ComponentModel.DataAnnotations;

namespace Planera.Api.Data.Users;

public class PersonalAccessToken
{
    [Key]
    [StringLength(250)]
    public required string UserId { get; set; }

    [StringLength(250)]
    public required string Secret { get; set; }

    public required DateTime CreatedAtUtc { get; set; }
}
