using System.ComponentModel.DataAnnotations;
using Planera.Data;

namespace Planera.Models.Ticket;

public class EditNoteModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(1024, ErrorMessage = ErrorStrings.StringLength)]
    public required string Content { get; init; }

    public required TicketStatus? Status { get; init; }
}