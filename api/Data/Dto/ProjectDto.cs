using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data.Dto;

public class ProjectDto
{
    public int Id { get; init; }

    public required string Slug { get; init; }

    public required string Name { get; init; }

    public UserDto Author { get; init; } = null!;
}