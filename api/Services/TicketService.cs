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

    public TicketService(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TicketDto>> AddTicketAsync(
        int projectId,
        string authorId,
        string title,
        string description,
        TicketPriority priority,
        IEnumerable<string> assigneeIds)
    {
        var project = await _dataContext.Projects.FindAsync(projectId);
        if (project == null)
            return Error.NotFound("ProjectId.NotFound", "A project with the given slug was not found.");

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
            AuthorId = authorId,
        };
        await _dataContext.Tickets.AddAsync(ticket);
        await _dataContext.SaveChangesAsync();

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<ErrorOr<Updated>> SetStatus(string currentUserId, int projectId, int ticketId, TicketStatus status)
    {
        var project = await _dataContext.Projects.FindAsync(projectId);
        if (project == null)
            return Error.NotFound("ProjectId.NotFound", "A project with the given slug was not found.");

        if (project.AuthorId != currentUserId)
            return Error.NotFound("NotAllowed", "You are not allowed to perform this action.");

        var ticket = await _dataContext.Tickets.FindAsync(ticketId, projectId);
        if (ticket == null)
            return Error.NotFound("TicketId.NotFound", "A ticket with the given ID was not found.");

        ticket.Status = status;
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}