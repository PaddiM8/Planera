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

    public required string AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}