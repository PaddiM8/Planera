using Planera.Api.Models.Ticket;

namespace Planera.Api.Data.Dto;

public record TicketQueryResult(
    IEnumerable<TicketDto> Tickets,
    TicketSorting Sorting,
    TicketFilter? Filter
);