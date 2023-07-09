using System.ComponentModel.DataAnnotations;
using Planera.Data;

namespace Planera.Models;

public class CreateTaskModel
{
    [MinLength(2)]
    [StringLength(150)]
    public required string Title { get; init; }

    [StringLength(10000)]
    public required string Description { get; init; }

    public required Priority Priority { get; init; }

    public required IEnumerable<string> AssigneeIds { get; init; } = new List<string>();
}