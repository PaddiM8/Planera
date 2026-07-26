using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Planera.Api.Data.Projects;
using Planera.Api.Data.Users;

namespace Planera.Api.Data.Tickets;

[PrimaryKey(nameof(Id), nameof(ProjectId))]
[Index(nameof(ProjectId))]
[Index(nameof(Deadline))]
[Index(nameof(Priority))]
[Index(nameof(Status))]
public class Ticket
{
    public int Id { get; set; }

    [StringLength(64)]
    public required string ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    [StringLength(150)]
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required TicketPriority Priority { get; set; }

    public ICollection<User> Assignees { get; set; } = new List<User>();

    public ICollection<Note> Notes { get; set; } = new List<Note>();

    [StringLength(64)]
    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public TicketStatus Status { get; set; }

    public required DateTime Timestamp { get; set; }
    
    public DateTime? ModifiedTimestamp { get; set; }
    
    public DateTime? Deadline { get; set; }
}