using Planera.Models.Ticket;

namespace Planera.Data.Dto;

public record TicketQueryResult(
    IEnumerable<TicketDto> Tickets,
    TicketSorting Sorting,
    TicketFilter? Filter
);