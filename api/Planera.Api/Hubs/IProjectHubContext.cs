using Planera.Api.Data.Dto;

namespace Planera.Api.Hubs;

public interface IProjectHubContext
{
    public Task OnRemoveTicket(string projectId, int ticketId);

    public Task OnUpdateTicket(string projectId, int ticketId, Dictionary<string, object> newFields);

    public Task OnAddParticipant(UserDto user);

    public Task OnRemoveParticipant(string name);

    public Task OnRemoveNote(int noteId);
}