using Planera.Api.Data.Notifications;
using Planera.Api.Data.Tickets;

namespace Planera.Api.Data.Dto;

public class ProjectParticipantDto
{
    public required string ProjectId { get; set; }

    public required UserDto User { get; set; }

    public TicketSorting Sorting { get; set; }

    public TicketFilter? Filter { get; set; }

    public List<NotificationKinds> EnabledNotificationKinds { get; set; } = [];
}