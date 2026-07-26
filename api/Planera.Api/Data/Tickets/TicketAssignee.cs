using Planera.Api.Data.Users;

namespace Planera.Api.Data.Tickets;

public class TicketAssignee
{
    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}