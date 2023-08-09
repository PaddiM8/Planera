namespace Planera.Data;

public class TicketAssignee
{
    public int ProjectId { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}