using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class EditProjectModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(100, ErrorMessage = ErrorStrings.StringLength)]
    public required string Name { get; init; }

    [StringLength(150, ErrorMessage = ErrorStrings.StringLength)]
    public required string Description { get; init; }
}