using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

[PrimaryKey(nameof(Id), nameof(ProjectId))]
public class Ticket
{
    public int Id { get; set; }

    public required int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required TicketPriority Priority { get; set; }

    public ICollection<User> Assignees { get; set; } = new List<User>();

    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public TicketStatus Status { get; set; }
}