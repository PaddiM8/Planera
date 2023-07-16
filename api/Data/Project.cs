using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

[Index(nameof(AuthorId), nameof(Slug), IsUnique = true)]
public class Project
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string Slug { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public string? IconPath { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public ICollection<User> Participants { get; set; } = new List<User>();

    public ICollection<User> InvitedUsers { get; set; } = new List<User>();
}