namespace Planera.Api.Data.Tickets;

public enum TicketFilter
{
    All = 0,
    Open = 1,
    Closed = 2,
    Inactive = 3,
    Done = 4,
    AssignedToMe = 5,
    OpenWithDeadline = 6,
}