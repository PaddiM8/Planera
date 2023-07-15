using Planera.Data.Dto;

namespace Planera.Hubs;

public interface IProjectHubContext
{
    public Task OnTicketUpdate(int projectId, int ticketId, Dictionary<string, object> newFields);

    public Task OnAddParticipant(UserDto user);

    public Task OnRemoveParticipant(string name);
}