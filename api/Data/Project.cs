using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data;

[Index(nameof(AuthorId), nameof(Slug), IsUnique = true)]
public class Project
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public required string Slug { get; set; }

    public required string Name { get; set; }

    [ForeignKey(nameof(Author))]
    public required string AuthorId { get; set; }

    [JsonIgnore]
    public User Author { get; set; } = null!;
}