using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planera.Data.Dto;

public class ProjectDto
{
    public required string Id { get; init; }

    public required string Slug { get; init; }

    public required string Name { get; init; }

    public required string Description { get; set; }

    public UserDto Author { get; init; } = null!;

    public string? IconPath { get; set; }

    public bool EnableTicketDescriptions { get; set; } = true;

    public bool EnableTicketAssignees { get; set; } = true;

    public required DateTime Timestamp { get; set; }

    public int AllTicketsCount { get; set; }

    public int OpenTicketsCount { get; set; }

    public int ClosedTicketsCount { get; set; }

    public int InactiveTicketsCount { get; set; }

    public int DoneTicketsCount { get; set; }

    /// <summary>
    /// Manually assigned
    /// </summary>
    public int AssignedToMeCount { get; set; }

    public ICollection<UserDto> Participants { get; set; } = new List<UserDto>();
}