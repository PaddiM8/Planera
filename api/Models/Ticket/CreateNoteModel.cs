using System.ComponentModel.DataAnnotations;

namespace Planera.Models.Ticket;

public class CreateNoteModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(1024, ErrorMessage = ErrorStrings.StringLength)]
    public required string Content { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    public required int TicketId { get; init; }

    [Required(ErrorMessage = ErrorStrings.Required)]
    public required string ProjectId { get; init; }
}