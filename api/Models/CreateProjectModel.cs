using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class CreateProjectModel
{
    [MinLength(2)]
    [StringLength(100)]
    public required string Name { get; init; }

    [RegularExpression(@"[a-z0-9\-]+")]
    [MinLength(2)]
    [StringLength(100)]
    public required string Slug { get; init; }
}