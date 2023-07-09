using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data.Dto;

public class ProjectDto
{
    public int Id { get; init; }

    public required string Slug { get; init; }

    public required string Name { get; init; }

    public required string Description { get; set; }

    public UserDto Author { get; init; } = null!;

    public ICollection<UserDto> Participants { get; set; } = new List<UserDto>();
}