using System.ComponentModel.DataAnnotations;

namespace Planera.Models;

public class EditProjectModel
{
    [MinLength(2)]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(150)]
    public required string Description { get; init; }
}