using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Planera.Api.Data.Notifications;
using Planera.Api.Data.Projects;
using Planera.Api.Data.Tickets;

namespace Planera.Api.Data.Users;

public class User : IdentityUser
{
    [StringLength(250)]
    public string? AvatarPath { get; set; }

    public InterfaceTheme Theme { get; set; }

    public NotificationKinds EnabledNotificationKinds { get; set; } = NotificationKinds.Core |
        NotificationKinds.DeadlineMyTicket |
        NotificationKinds.DeadlineOtherTicket;

    public List<string> PinnedProjects { get; set; } = [];

    public ICollection<Project> Projects { get; init; } = new List<Project>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Project> JoinedProjects { get; set; } = new List<Project>();

    public ICollection<Project> Invitations { get; set; } = new List<Project>();

    public ICollection<ProjectParticipant> ProjectParticipations { get; set; } = new List<ProjectParticipant>();
}