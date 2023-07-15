using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class CreateProjectModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(100, ErrorMessage = ErrorStrings.StringLength)]
    public required string Name { get; init; }

    [RegularExpression(@"[a-z0-9\-]+")]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(100, ErrorMessage = ErrorStrings.StringLength)]
    public required string Slug { get; init; }

    [StringLength(150, ErrorMessage = ErrorStrings.StringLength)]
    public required string Description { get; init; }
}