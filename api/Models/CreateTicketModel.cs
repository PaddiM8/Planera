using System.ComponentModel.DataAnnotations;
using Planera.Data;

namespace Planera.Models;

public class CreateTicketModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(150, ErrorMessage = ErrorStrings.StringLength)]
    public required string Title { get; init; }

    [StringLength(10000, ErrorMessage = ErrorStrings.StringLength)]
    public required string Description { get; init; }

    public required TicketPriority Priority { get; init; }

    public required IEnumerable<string> AssigneeIds { get; init; } = new List<string>();
}