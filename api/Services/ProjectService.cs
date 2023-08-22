using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Data.Files;

namespace Planera.Services;

public class ProjectService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly ILookupNormalizer _normalizer;
    private readonly IFileStorage _fileStorage;

    public ProjectService(
        DataContext dataContext,
        IMapper mapper,
        UserManager<User> userManager,
        ILookupNormalizer normalizer,
        IFileStorage fileStorage)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _userManager = userManager;
        _normalizer = normalizer;
        _fileStorage = fileStorage;
    }

    public static ErrorOr<T> ProjectNotFoundError<T>()
        => Error.Conflict("Project.NotFound", "Project was not found.");

    public IQueryable<Project> QueryById(string userId, int projectId)
    {
        return _dataContext.Projects
            .Where(x => x.Id == projectId)
            .Where(x => x.Participants.Any(user => user.Id == userId));
    }

    public IQueryable<Project> QueryBySlug(string userId, string authorName, string slug)
    {
        return _dataContext.Projects
            .Where(x => x.Author.NormalizedUserName == _normalizer.NormalizeName(authorName))
            .Where(x => x.Slug == slug)
            .Where(x => x.Participants.Any(user => user.Id == userId));
    }

    public async Task<ErrorOr<ICollection<ProjectDto>>> GetAllAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return Error.NotFound("Username.NotFound", "A user with the provided name was not found.");

        return await _dataContext.Projects
            .Where(a => a.Participants.Contains(user))
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<ProjectDto>> GetAsync(
        string userId,
        string authorName,
        string slug)
    {
        var project = await QueryBySlug(userId, authorName, slug)
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
        string description,
        string? icon)
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
            Slug = slug.ToLower(),
            Name = name,
            Description = description,
            Participants = new List<User> { author },
        };


        await using var transaction = await _dataContext.Database.BeginTransactionAsync();
        await _dataContext.Projects.AddAsync(project);
        await _dataContext.SaveChangesAsync();
        _fileStorage.CreateDirectory(project.Id.ToString());

        if (icon?.StartsWith("data:") is not true)
            return project.Id;

        // Expected format of icon: `data:image/png;base64,BASE64STRING==`
        var bytes = Convert.FromBase64String(icon.Split(",")[1]);
        var avatar256 = ImagePreparer.Resize(bytes, 256, 256);
        var avatar32 = ImagePreparer.Resize(bytes, 32, 32);
        project.IconPath = await _fileStorage.WriteManyAsync(
            project.Id.ToString(),
            (avatar256, "256"),
            (avatar32, "32")
        );

        try
        {
            _dataContext.Update(project);
            await _dataContext.SaveChangesAsync();
        }
        catch
        {
            // If it didn't update, remove the newly created files, since
            // they won't be used.
            if (!string.IsNullOrEmpty(project.IconPath))
            {
                _fileStorage.Delete(project.IconPath, "32");
                _fileStorage.Delete(project.IconPath, "256");
            }

            await transaction.CommitAsync();

            return Error.Unexpected("Unknown", "Failed to create project.");
        }

        await transaction.CommitAsync();

        return project.Id;
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        string authorName,
        string slug,
        string name,
        string description,
        string? icon)
    {
        var project = await QueryBySlug(userId, authorName, slug)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Updated>();

        var previousIconPath = project.IconPath;
        if (icon?.StartsWith("data:") is true)
        {
            // Expected format of icon: `data:image/png;base64,BASE64STRING==`
            var bytes = Convert.FromBase64String(icon.Split(",")[1]);
            var avatar256 = ImagePreparer.Resize(bytes, 256, 256);
            var avatar32 = ImagePreparer.Resize(bytes, 32, 32);
            project.IconPath = await _fileStorage.WriteManyAsync(
                project.Id.ToString(),
                (avatar256, "256"),
                (avatar32, "32")
            );
        }
        else if (icon == "")
        {
            project.IconPath = null;
        }

        project.Name = name;
        project.Description = description;

        _dataContext.Projects.Update(project);
        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch
        {
            // If it didn't update, remove the newly created files, since
            // they won't be used.
            if (!string.IsNullOrEmpty(project.IconPath))
            {
                _fileStorage.Delete(project.IconPath, "32");
                _fileStorage.Delete(project.IconPath, "256");
            }

            return Error.Unexpected("Unknown", "Failed to update project.");
        }

        if (previousIconPath != null && previousIconPath != project.IconPath)
        {
            _fileStorage.Delete(previousIconPath, "32");
            _fileStorage.Delete(previousIconPath, "256");
        }

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

        _fileStorage.DeleteDirectory(projectId.ToString());

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<(UserDto user, ProjectDto project)>> InviteParticipantAsync(
        string userId,
        int projectId,
        string participantName)
    {
        var project = await QueryById(userId, projectId)
            .Include(x => x.Author)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<(UserDto, ProjectDto)>();

        var participant = await _userManager.FindByNameAsync(participantName);
        if (participant == null)
            return Error.NotFound("Participant.NotFound", "A user with the given name was not found.");

        await _dataContext.Invitations.AddAsync(new Invitation
        {
            Project = project,
            User = participant,
        });
        await _dataContext.SaveChangesAsync();

        return (_mapper.Map<UserDto>(participant), _mapper.Map<ProjectDto>(project));
    }

    public async Task<ErrorOr<Deleted>> RemoveParticipantAsync(
        string userId,
        int projectId,
        string participantName)
    {
        var project = await QueryById(userId, projectId)
            .Include(x => x.Author)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Deleted>();

        var participant = await _dataContext.ProjectParticipants
            .Where(x => x.Project.Id == projectId)
            .Where(x => x.User.NormalizedUserName == _normalizer.NormalizeName(participantName))
            .Include(x => x.User)
            .SingleOrDefaultAsync();
        if (participant == null)
            return await RemoveInvitation(projectId, participantName);

        if (participant.User.Id == project.Author.Id)
            return Error.Conflict("ParticipantName.NotAllowed", "Cannot remove project author.");

        _dataContext.ProjectParticipants.Remove(participant);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    private async Task<ErrorOr<Deleted>> RemoveInvitation(int projectId, string inviteeName)
    {
        var invitation = await _dataContext.Invitations
            .Where(x => x.User.NormalizedUserName == _normalizer.NormalizeName(inviteeName))
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