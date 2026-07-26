using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Planera.Api.Data.Tickets;
using Planera.Api.Data.Users;

namespace Planera.Api.Data.Projects;

[Index(nameof(AuthorId), nameof(Slug), IsUnique = true)]
public class Project
{
    [StringLength(64)]
    public required string Id { get; set; }

    [StringLength(64)]
    public required string Slug { get; set; }

    [StringLength(120)]
    public required string Name { get; set; }

    [StringLength(250)]
    public required string Description { get; set; }

    [StringLength(64)]
    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;

    [StringLength(256)]
    public string? IconPath { get; set; }

    public required DateTime Timestamp { get; set; }

    public bool EnableTicketDescriptions { get; set; } = true;

    public bool EnableTicketAssignees { get; set; } = true;
    
    public bool EnableTicketDeadlines { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<User> Participants { get; set; } = new List<User>();

    public ICollection<User> InvitedUsers { get; set; } = new List<User>();
    
    public ICollection<NotificationTrigger> NotificationTriggers { get; set; } = new List<NotificationTrigger>();
}