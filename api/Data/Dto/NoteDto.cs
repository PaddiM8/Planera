namespace Planera.Data.Dto;

public class NoteDto
{
    public int Id { get; set; }
    
    public required string Content { get; set; }
    
    public required DateTime Timestamp { get; set; }

    public TicketStatus Status { get; set; }

    public ProjectDto Project { get; set; } = null!;

    public UserDto Author { get; set; } = null!;
}