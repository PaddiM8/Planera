using Planera.Api.Models.Ticket;

namespace Planera.Api.Data;

public class ProjectParticipant
{
    public string ProjectId { get; set; } = null!;

    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;

    public TicketSorting Sorting { get; set; }

    public TicketFilter? Filter { get; set; }
}