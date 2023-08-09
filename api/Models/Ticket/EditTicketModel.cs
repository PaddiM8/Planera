using System.ComponentModel.DataAnnotations;
using Planera.Data;

namespace Planera.Models.Ticket;

public class EditTicketModel
{
    [MinLength(2, ErrorMessage = ErrorStrings.MinLength)]
    [StringLength(150, ErrorMessage = ErrorStrings.StringLength)]
    public required string Title { get; init; }

    [StringLength(10000, ErrorMessage = ErrorStrings.StringLength)]
    public required string Description { get; init; }
}