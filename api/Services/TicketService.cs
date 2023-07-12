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

    public async Task<ErrorOr<Updated>> SetStatus(
        string userId,
        int projectId,
        int ticketId,
        TicketStatus status)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<Updated>();

        var ticket = await _dataContext.Tickets.FindAsync(ticketId, projectId);
        if (ticket == null)
        {
            return Error.NotFound(
                "TicketId.NotFound",
                "A ticket with the given ID was not found."
            );
        }

        ticket.Status = status;
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}