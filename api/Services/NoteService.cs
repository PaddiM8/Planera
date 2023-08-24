using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Services;

public class NoteService
{
    private readonly ProjectService _projectService;
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public NoteService(ProjectService projectService, DataContext dataContext, IMapper mapper)
    {
        _projectService = projectService;
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public static ErrorOr<T> NoteNotFoundError<T>()
        => Error.Conflict("Note.NotFound", "Note was not found.");

    public async Task<ErrorOr<List<NoteDto>>> GetAllAsync(string userId, string projectId, int ticketId)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .FirstOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<List<NoteDto>>();

        return await _dataContext.Notes
            .Where(x => x.ProjectId == projectId && x.TicketId == ticketId)
            .ProjectTo<NoteDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<NoteDto>> AddAsync(string userId, string projectId, int ticketId, string content)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .FirstOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<NoteDto>();

        var ticket = await _dataContext.Tickets.FindAsync(ticketId, projectId);
        if (ticket == null)
            return ProjectService.ProjectNotFoundError<NoteDto>();

        var note = new Note
        {
            Content = content,
            Timestamp = DateTime.Now,
            TicketId = ticketId,
            ProjectId = projectId,
            AuthorId = userId,
        };
        await _dataContext.Notes.AddAsync(note);
        await _dataContext.SaveChangesAsync();

        return _mapper.Map<NoteDto>(note);
    }

    public async Task<ErrorOr<Deleted>> RemoveAsync(string userId, int id)
    {
        var note = await _dataContext.Notes.FindAsync(id);
        if (note == null)
            return NoteNotFoundError<Deleted>();

        // Make sure the user has access to this project
        var project = await _projectService
            .QueryById(userId, note.ProjectId)
            .FirstOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<Deleted>();

        _dataContext.Remove(note);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        int id,
        string? content,
        TicketStatus? status)
    {
        var note = await _dataContext.Notes.FindAsync(id);
        if (note == null)
            return NoteNotFoundError<Updated>();

        // Make sure the user has access to this project
        var project = await _projectService
            .QueryById(userId, note.ProjectId)
            .FirstOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<Updated>();

        if (content != null)
            note.Content = content;

        if (status != null)
            note.Status = status.Value;

        _dataContext.Update(note);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}