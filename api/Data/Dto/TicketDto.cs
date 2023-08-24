namespace Planera.Data.Dto;

public class TicketDto
{
    public required int Id { get; set; }

    public required string ProjectId { get; set; }

    public required string ProjectSlug { get; set; }

    public ProjectDto? Project { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required TicketPriority Priority { get; set; }

    public required ICollection<UserDto> Assignees { get; set; }

    public required ICollection<NoteDto> Notes { get; set; }

    public required UserDto Author { get; set; }

    public TicketStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
}