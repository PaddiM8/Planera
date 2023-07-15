using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

public class User : IdentityUser
{
    public ICollection<Project> Projects { get; init; } = new List<Project>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Project> JoinedProjects { get; set; } = new List<Project>();

    public ICollection<Project> Invitations { get; set; } = new List<Project>();
}