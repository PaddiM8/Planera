using System.ComponentModel.DataAnnotations;

namespace Planera.Data;

public class PersonalAccessToken
{
    [Key]
    public required string UserId { get; set; }

    public required string Secret { get; set; }

    public required DateTime CreatedAtUtc { get; set; }
}
