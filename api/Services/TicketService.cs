using AutoMapper;
using Planera.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Planera.Data.Dto;

namespace Planera.Services;

public class TicketService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly ProjectService _projectService;

    public TicketService(DataContext dataContext, IMapper mapper, ProjectService projectService)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _projectService = projectService;
    }

    private async Task<ErrorOr<Ticket>> FindAsync(string userId, int projectId, int ticketId)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<Ticket>();

        var ticket = await _dataContext.Tickets.FindAsync(ticketId, projectId);

        return ticket == null
            ? Error.NotFound("TicketId.NotFound", "A ticket with the given ID was not found.")
            : ticket;
    }

    public async Task<ErrorOr<TicketDto>> GetAsync(
        string userId,
        string username,
        string slug,
        int ticketId)
    {
        var project = await _projectService
            .QueryBySlug(userId, username, slug)
            .Include(x => x.Author)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<TicketDto>();

        var ticket = await _dataContext.Tickets
            .Where(x => x.Id == ticketId && x.ProjectId == project.Id)
            .Include(x => x.Assignees)
            .SingleOrDefaultAsync();
        if (ticket == null)
            return Error.NotFound("TicketId.NotFound", "A ticket with the given ID was not found.");

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<ErrorOr<TicketDto>> AddTicketAsync(
        string userId,
        int projectId,
        string title,
        string description,
        TicketPriority priority,
        IEnumerable<string> assigneeIds)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<TicketDto>();

        int id = await _dataContext.Tickets
            .Where(x => x.ProjectId == project.Id)
            .CountAsync() + 1;
        var assignees = await _dataContext.Users
            .Where(x => assigneeIds.Contains(x.Id))
            .ToListAsync();
        var ticket = new Ticket
        {
            Id = id,
            ProjectId = project.Id,
            Title = title,
            Description = description,
            Priority = priority,
            Assignees = assignees,
            AuthorId = userId,
        };
        await _dataContext.Tickets.AddAsync(ticket);
        await _dataContext.SaveChangesAsync();

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<ErrorOr<Updated>> EditTicketAsync(
        string userId,
        int projectId,
        int ticketId,
        string title,
        string description)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var ticket = ticketResult.Value;
        ticket.Title = title;
        ticket.Description = description;
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Deleted>> RemoveTicketAsync(
        string userId,
        int projectId,
        int ticketId)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var ticket = ticketResult.Value;
        _dataContext.Tickets.Remove(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<Updated>> SetStatus(
        string userId,
        int projectId,
        int ticketId,
        TicketStatus status)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var ticket = ticketResult.Value;
        ticket.Status = status;
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Updated>> SetPriorityAsync(
        string userId,
        int projectId,
        int ticketId,
        TicketPriority priority)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var ticket = ticketResult.Value;
        ticket.Priority = priority;
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Updated>> AddAssigneeAsync(
        string userId,
        int projectId,
        int ticketId,
        string assigneeId)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var assignee = await _dataContext.Users.FindAsync(assigneeId);
        if (assignee == null)
            return Error.NotFound("AssigneeId.NotFound", "A user with the given ID was not found.");

        await _dataContext.TicketAssignees.AddAsync(
            new TicketAssignee
            {
                Ticket = ticketResult.Value,
                User = assignee,
            }
        );
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Deleted>> RemoveAssigneeAsync(
        string userId,
        int projectId,
        int ticketId,
        string assigneeId)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var assignee = await _dataContext.TicketAssignees
            .Where(x => x.Ticket.Project.Id == projectId)
            .Where(x => x.Ticket.Id == ticketId)
            .Where(x => x.User.Id == assigneeId)
            .SingleOrDefaultAsync();
        if (assignee == null)
            return Error.NotFound("AssigneeId.NotFound", "A user with the given ID was not found in this project.");

        _dataContext.TicketAssignees.Remove(assignee);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }
}