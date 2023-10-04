using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Models.Ticket;
using Planera.Services;

namespace Planera.Hubs;

[Authorize]
public class ProjectHub : Hub<IProjectHubContext>
{
    private readonly ProjectService _projectService;
    private readonly TicketService _ticketService;
    private readonly NoteService _noteService;
    private readonly IHubContext<UserHub, IUserHubContext> _userHub;

    public ProjectHub(
        ProjectService projectService,
        TicketService ticketService,
        NoteService noteService,
        IHubContext<UserHub, IUserHubContext> userHub)
    {
        _projectService = projectService;
        _ticketService = ticketService;
        _noteService = noteService;
        _userHub = userHub;
    }

    public async Task Join(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task Leave(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task<TicketQueryResult> QueryTickets(
        string username,
        string slug,
        int startIndex,
        int amount,
        string? query = null,
        TicketSorting sorting = TicketSorting.Newest,
        TicketFilter? filter = null)
    {
        var result = await _ticketService.GetAllAsync(
            Context.User!.FindFirst("Id")!.Value,
            username,
            slug,
            startIndex,
            amount,
            query,
            sorting,
            filter
        );

        return result.Unwrap();
    }

    public async Task RemoveTicket(string projectId, int ticketId)
    {
        var result = await _ticketService.RemoveTicketAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            ticketId
        );
        result.Unwrap();

        await Clients
            .Group(projectId)
            .OnRemoveTicket(projectId, ticketId);
    }

    public async Task SetTicketStatus(string projectId, int ticketId, TicketStatus status)
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
            .Group(projectId)
            .OnUpdateTicket(projectId, ticketId, newFields);
    }

    public async Task SetTicketPriority(string projectId, int ticketId, TicketPriority priority)
    {
        var result = await _ticketService.SetPriorityAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            priority
        );
        result.Unwrap();

        var newFields = new Dictionary<string, object>
        {
            { nameof(TicketDto.Priority), priority },
        };
        await Clients
            .Group(projectId)
            .OnUpdateTicket(projectId, ticketId, newFields);
    }

    public async Task AddTicketAssignee(string projectId, int ticketId, string assigneeId)
    {
        var result = await _ticketService.AddAssigneeAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            assigneeId
        );

        var newFields = new Dictionary<string, object>
        {
            { nameof(TicketDto.Assignees), result.Unwrap() },
        };
        await Clients
            .Group(projectId)
            .OnUpdateTicket(projectId, ticketId, newFields);
    }

    public async Task RemoveTicketAssignee(string projectId, int ticketId, string assigneeId)
    {
        var result = await _ticketService.RemoveAssigneeAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            ticketId,
            assigneeId
        );
        result.Unwrap();
    }

    public async Task Invite(string projectId, string username)
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

    public async Task RemoveParticipant(string projectId, string username)
    {
        var result = await _projectService.RemoveParticipantAsync(
            Context.User!.FindFirst("Id")!.Value,
            projectId,
            username
        );
        result.Unwrap();

        await Clients
            .Group(projectId)
            .OnRemoveParticipant(username);
    }

    public async Task RemoveNote(int noteId)
    {
        var result = await _noteService.RemoveAsync(
            Context.User!.FindFirst("Id")!.Value,
            noteId
        );
        result.Unwrap();
    }

    public async Task SetNoteStatus(int noteId, TicketStatus status)
    {
        var result = await _noteService.EditAsync(
            Context.User!.FindFirst("Id")!.Value,
            noteId,
            null,
            status
        );
        result.Unwrap();
    }
}