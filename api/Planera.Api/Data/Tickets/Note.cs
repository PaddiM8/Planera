using System.ComponentModel.DataAnnotations.Schema;
using Planera.Api.Data.Users;

namespace Planera.Api.Data.Tickets;

public class Note
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string Content { get; set; }

    public required DateTime Timestamp { get; set; }

    public TicketStatus Status { get; set; }

    public required string ProjectId { get; set; }

    public required int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;
}