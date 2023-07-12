using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Services;

public class ProjectService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public ProjectService(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public static ErrorOr<T> ProjectNotFoundError<T>()
        => Error.Conflict("Project.NotFound", "Project was not found.");

    public IQueryable<Project> QueryById(string userId, int projectId)
    {
        return _dataContext.Projects
            .Where(x => x.Id == projectId)
            .Where(x => x.Participants.Any(user => user.Id == userId));
    }

    public IQueryable<Project> QueryBySlug(
        string authorName,
        string slug,
        string userId)
    {
        return _dataContext.Projects
            .Where(x => x.Author.UserName == authorName)
            .Where(x => x.Slug == slug)
            .Where(x => x.Participants.Any(user => user.Id == userId));
    }

    public async Task<ErrorOr<ICollection<ProjectDto>>> GetAllAsync(string username)
    {
        return await _dataContext.Projects
            .Where(a => a.Participants.Any(b => b.UserName == username))
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<ProjectDto>> GetAsync(
        string userId,
        string authorName,
        string slug)
    {
        var project = await QueryBySlug(authorName, slug, userId)
            .Include(x => x.Participants)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        if (project == null)
        {
            return Error.NotFound(
                "Slug.NotFound",
                "A project with the given slug was not found."
            );
        }

        return project;
    }

    public async Task<ErrorOr<int>> AddAsync(
        string authorId,
        string slug,
        string name,
        string description)
    {
        if (await _dataContext.Projects.AnyAsync(x => x.Slug == slug))
        {
            return Error.Conflict(
                "Slug.AlreadyExists",
                "A project with the given slug already exists."
            );
        }

        var author = await _dataContext.Users.FindAsync(authorId);
        if (author == null)
            return Error.Conflict("User.NotFound", "User was not found.");

        var project = new Project
        {
            AuthorId = authorId,
            Slug = slug,
            Name = name,
            Description = description,
            Participants = new List<User> { author },
        };
        await _dataContext.Projects.AddAsync(project);
        await _dataContext.SaveChangesAsync();

        return project.Id;
    }

    public async Task<ErrorOr<Updated>> EditAsync(string userId, string authorName,
        string slug,
        string name,
        string description)
    {
        var project = await QueryBySlug(authorName, slug, userId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Updated>();

        project.Name = name;
        project.Description = description;

        _dataContext.Projects.Update(project);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Deleted>> RemoveAsync(string userId, int projectId)
    {
        var project = await QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Deleted>();

        _dataContext.Projects.Remove(project);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<ICollection<TicketDto>>> GetTicketsAsync(
        string userId,
        int projectId)
    {
        var project = await QueryById(userId, projectId)
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Author)
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Assignees)
            .SingleOrDefaultAsync();

        return project == null
            ? ProjectNotFoundError<ICollection<TicketDto>>()
            : ErrorOrFactory.From(_mapper.Map<ICollection<TicketDto>>(project.Tickets));
    }

    public async Task<ErrorOr<UserDto>> AddParticipantAsync(
        string userId,
        int projectId,
        string participantName)
    {
        var project = await QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<UserDto>();

        var participant = await _dataContext.Users
            .Where(x => x.UserName == participantName)
            .SingleOrDefaultAsync();
        if (participant == null)
        {
            return Error.NotFound(
                "Participant.NotFound",
                "A user with the given name was not found."
            );
        }

        await _dataContext.Invitations.AddAsync(new Invitation
        {
            Project = project,
            User = participant,
        });
        await _dataContext.SaveChangesAsync();

        return _mapper.Map<UserDto>(participant);
    }

    public async Task<ErrorOr<Deleted>> RemoveParticipantAsync(
        string userId,
        int projectId,
        string participantName)
    {
        var project = await QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Deleted>();

        var participant = await _dataContext.ProjectParticipants
            .Where(x => x.Project.Id == projectId)
            .Where(x => x.User.UserName == participantName)
            .SingleOrDefaultAsync();
        if (participant == null)
            return await RemoveInvitation(projectId, participantName);

        _dataContext.ProjectParticipants.Remove(participant);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    private async Task<ErrorOr<Deleted>> RemoveInvitation(int projectId, string inviteeName)
    {
        var invitation = await _dataContext.Invitations
            .Where(x => x.User.UserName == inviteeName)
            .Where(x => x.ProjectId == projectId)
            .SingleOrDefaultAsync();
        if (invitation == null)
        {
            return Error.NotFound(
                "User.NotFound",
                "A user with the given name was not found in the current project."
            );
        }

        _dataContext.Invitations.Remove(invitation);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }
}