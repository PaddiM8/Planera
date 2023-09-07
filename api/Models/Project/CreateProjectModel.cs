using System.ComponentModel.DataAnnotations;

namespace Planera.Models.Project;

public class CreateProjectModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(100, ErrorMessage = ErrorStrings.StringLength)]
    public required string Name { get; init; }

    [RegularExpression(@"[a-z0-9\-]+", ErrorMessage = "Slugs can only contains letters, digits and dashes.")]
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(64, ErrorMessage = ErrorStrings.StringLength)]
    public required string Slug { get; init; }

    [StringLength(150, ErrorMessage = ErrorStrings.StringLength)]
    public required string Description { get; init; }

    public string? Icon { get; init; }
}