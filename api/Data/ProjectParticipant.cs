namespace Planera.Data;

public class ProjectParticipant
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}