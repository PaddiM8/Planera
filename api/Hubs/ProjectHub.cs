using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Services;

namespace Planera.Hubs;

[Authorize]
public class ProjectHub : Hub<IProjectHubContext>
{
    private readonly ProjectService _projectService;
    private readonly TicketService _ticketService;
    private readonly IHubContext<UserHub, IUserHubContext> _userHub;

    public ProjectHub(
        ProjectService projectService,
        TicketService ticketService,
        IHubContext<UserHub, IUserHubContext> userHub)
    {
        _projectService = projectService;
        _ticketService = ticketService;
        _userHub = userHub;
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
        result.Unwrap();

        var newFields = new Dictionary<string, object>
        {
            { nameof(TicketDto.Status), status },
        };
        await Clients
            .Group(projectId.ToString())
            .OnTicketUpdate(projectId, ticketId, newFields);
    }

    public async Task Invite(int projectId, string username)
    {
        var result = await _projectService.InviteParticipantAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            username
        );

        await _userHub.Clients
            .User(username)
            .OnAddInvitation(result.Unwrap().project);
    }

    public async Task RemoveParticipant(int projectId, string username)
    {
        var result = await _projectService.RemoveParticipantAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            username
        );
        result.Unwrap();

        await Clients
            .Group(projectId.ToString())
            .OnRemoveParticipant(username);
    }
}