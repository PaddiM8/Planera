using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Planera.Data;
using Planera.Services;

namespace Planera.Hubs;

[Authorize]
public class ProjectHub : Hub
{
    private readonly TicketService _ticketService;

    // TODO: Create a UserHub that receives and handles invitations, which are
    // sent by the ProjectController (if possible, otherwise the ProjectHub)?
    // Also might want to store the hub clients in a svelte store or something
    // on the client.
    public ProjectHub(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    public async Task Join(int projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId.ToString());
    }

    public async Task Leave(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId.ToString());
    }

    public async Task SetTicketStatus(int projectId, int ticketId, TicketStatus status)
    {
        var result = await _ticketService.SetStatus(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            status
        );
        if (result.IsError)
            throw new HubException(result.Errors.FirstOrDefault().Description);

        await Clients
            .Group(projectId.ToString())
            .SendAsync("getTicketUpdate", projectId, ticketId, new { Status = status });
    }
}