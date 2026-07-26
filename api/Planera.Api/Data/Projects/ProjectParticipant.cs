using System.ComponentModel.DataAnnotations;
using Planera.Api.Data.Tickets;
using Planera.Api.Data.Users;

namespace Planera.Api.Data.Projects;

public class ProjectParticipant
{
    [StringLength(64)]
    public string ProjectId { get; set; } = null!;

    public Project Project { get; set; } = null!;

    [StringLength(64)]
    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;

    public TicketSorting Sorting { get; set; }

    public TicketFilter? Filter { get; set; }
}