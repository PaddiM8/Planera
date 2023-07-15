namespace Planera.Data.Dto;

public class InvitationDto
{
    public ProjectDto Project { get; set; } = null!;

    public UserDto User { get; set; } = null!;
}