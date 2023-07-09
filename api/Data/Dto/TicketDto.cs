namespace Planera.Data.Dto;

public class TicketDto
{
    public required int Id { get; set; }

    public required int ProjectId { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required Priority Priority { get; set; }

    public required ICollection<UserDto> Assignees { get; set; }

    public required UserDto Author { get; set; }
}