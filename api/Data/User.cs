using Microsoft.AspNetCore.Identity;

namespace Planera.Data;

public class User : IdentityUser
{
    public override string UserName
        => base.UserName!;

    public override string Email
        => base.Email!;

    public ICollection<Project> Projects { get; init; } = new List<Project>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Project> JoinedProjects { get; set; } = new List<Project>();
}