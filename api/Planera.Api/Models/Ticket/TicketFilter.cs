namespace Planera.Api.Models.Ticket;

public enum TicketFilter
{
    All,
    Open,
    OpenWithDeadline,
    Closed,
    Inactive,
    Done,
    AssignedToMe,
}