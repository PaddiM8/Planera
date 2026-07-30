using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Planera.Api.Data;
using Planera.Api.Data.Dto;
using Planera.Api.Data.Files;
using Planera.Api.Data.Notifications;
using Planera.Api.Data.Projects;
using Planera.Api.Data.Users;
using Planera.Api.Utility;

namespace Planera.Api.Services;

public class ProjectService(
    DataContext dataContext,
    IMapper mapper,
    IConfiguration configuration,
    UserManager<User> userManager,
    ILookupNormalizer normalizer,
    NotificationScheduler notificationScheduler,
    IFileStorage fileStorage,
    EmailService emailService
)
{
    private readonly DataContext _dataContext = dataContext;
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _configuration = configuration;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILookupNormalizer _normalizer = normalizer;
    private readonly NotificationScheduler _notificationScheduler = notificationScheduler;
    private readonly IFileStorage _fileStorage = fileStorage;
    private readonly EmailService _emailService = emailService;

    public static ErrorOr<T> ProjectNotFoundError<T>()
        => Error.Conflict("Project.NotFound", "Project was not found.");

    public IQueryable<Project> QueryById(string userId, string projectId)
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
    
    public IQueryable<Project> QueryByUserId(string userId)
    {
        return _dataContext.Projects
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
            .Include(x => x.NotificationTriggers)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        if (project == null)
        {
            return Error.NotFound(
                "Slug.NotFound",
                "A project with the given slug was not found."
            );
        }

        project.AssignedToMeCount = _dataContext.Tickets
            .Count(x =>
                x.ProjectId == project.Id && x.Assignees.Any(assignee => assignee.Id == userId)
            );
        var me = await _dataContext
            .ProjectParticipants
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.UserId == userId);
        if (me != null)
            project.Me = _mapper.Map<ProjectParticipant, ProjectParticipantDto>(me);

        return project;
    }

    public async Task<ErrorOr<string>> AddAsync(
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

        var id = Guid.NewGuid().ToString();
        var project = new Project
        {
            Id = id,
            AuthorId = authorId,
            Slug = slug.ToLower(),
            Name = name,
            Description = description,
            Timestamp = DateTime.UtcNow,
            Participants = new List<User> { author },
            NotificationTriggers = GetDefaultNotificationTriggers(id),
        };

        var strategy = _dataContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<ErrorOr<string>>(async () =>
        {
            await using var transaction = await _dataContext.Database.BeginTransactionAsync();
            await _dataContext.Projects.AddAsync(project);
            await _dataContext.SaveChangesAsync();
            _fileStorage.CreateDirectory(project.Id);

            if (icon?.StartsWith("data:") is not true)
            {
                await transaction.CommitAsync();

                return project.Id;
            }

            // Expected format of icon: `data:image/png;base64,BASE64STRING==`
            var bytes = Convert.FromBase64String(icon.Split(",")[1]);
            var avatar256 = ImagePreparer.Resize(bytes, 256, 256);
            var avatar32 = ImagePreparer.Resize(bytes, 32, 32);
            project.IconPath = await _fileStorage.WriteManyAsync(
                project.Id,
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
        });
    }

    private List<NotificationTrigger> GetDefaultNotificationTriggers(string projectId)
    {
        var notificationTrigger = new NotificationTrigger
        {
            ProjectId = projectId,
            Trigger = NotificationTriggerKind.TimeUntilDeadline,
            Threshold = "1",
            ThresholdUnit = NotificationThresholdUnit.Days,
            Action = NotificationActionKind.PushNotification,
        };

        return [notificationTrigger];
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        string authorName,
        string slug,
        string name,
        string description,
        string? icon,
        bool? enableTicketDescriptions,
        bool? enableTicketAssignees,
        bool? enableTicketDeadlines)
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
                project.Id,
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

        if (enableTicketDescriptions != null)
            project.EnableTicketDescriptions = enableTicketDescriptions.Value;

        if (enableTicketAssignees != null)
            project.EnableTicketAssignees = enableTicketAssignees.Value;

        if (enableTicketDeadlines != null)
            project.EnableTicketDeadlines = enableTicketDeadlines.Value;

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

    public async Task<ErrorOr<Deleted>> RemoveAsync(string userId, string projectId)
    {
        var project = await QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Deleted>();

        var strategy = _dataContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await _dataContext
                .ProjectParticipants
                .Where(x => x.ProjectId == projectId)
                .ExecuteDeleteAsync();

            _dataContext.Projects.Remove(project);
            await _dataContext.SaveChangesAsync();
        });

        _fileStorage.DeleteDirectory(projectId);

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<(UserDto user, ProjectDto project)>> InviteParticipantAsync(
        string userId,
        string projectId,
        string participantName)
    {
        var project = await QueryById(userId, projectId)
            .Include(x => x.Author)
            .Include(x => x.Participants)
            .Include(x => x.InvitedUsers)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<(UserDto, ProjectDto)>();

        var normalizedParticipantName = _userManager.NormalizeName(participantName);
        if (project.Participants.Any(p => p.NormalizedUserName == normalizedParticipantName))
            return Error.Conflict("Participant.AlreadyAdded", "A user with the given name is already a participant in the project.");

        if (project.InvitedUsers.Any(p => p.NormalizedUserName == normalizedParticipantName))
            return Error.Conflict("Participant.AlreadyInvited", "A user with the given name has already been invited to the project.");

        var participant = await _userManager.FindByNameAsync(participantName);
        if (participant == null)
            return Error.NotFound("Participant.NotFound", "A user with the given name was not found.");

        await _dataContext.Invitations.AddAsync(new Invitation
        {
            Project = project,
            User = participant,
        });
        await _dataContext.SaveChangesAsync();

        var notificationEntry = new NotificationQueueEntry
        {
            TargetId = participant.Id,
            TargetKind = NotificationTargetKind.User,
            ActionKind = NotificationActionKind.PushNotification,
            Title = "Project Invitation",
            Content = $"You have been invited to '{project.Name.Truncate(25)}'.",
            NotificationKind = NotificationKinds.Core,
            ObjectId = project.Id,
            Url = "/invitations",
        };
        await _notificationScheduler.ScheduleAsync(notificationEntry, isNew: true);

        if (participant.Email != null && participant.EnabledNotificationKinds.HasFlag(NotificationKinds.Core))
        {
            var frontendUrl = _configuration["FrontendUrl"]!;
            var acceptUrl = $"{frontendUrl}/api/user/invitations/{project.Id}/accept";
            var declineUrl = $"{frontendUrl}/api/user/invitations/{project.Id}/decline";
            var emailBody = $"""
                <p>Would you like to join the project?</p>
                {EmailTemplateUtility.Button("Accept", acceptUrl, isPrimary: true)}
                {EmailTemplateUtility.Button("Decline", declineUrl, isPrimary: false)}
                """;
            await _emailService.SendAsync($"You've been invited to '{project.Name.Truncate(35)}'", emailBody, participant.Email);
        }

        return (_mapper.Map<UserDto>(participant), _mapper.Map<ProjectDto>(project));
    }

    public async Task<ErrorOr<Deleted>> RemoveParticipantAsync(
        string userId,
        string projectId,
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

    public async Task<ErrorOr<Updated>> SetNotificationTriggersAsync(string userId, string projectId, List<NotificationTriggerDto> triggers)
    {
        var project = await QueryById(userId, projectId)
            .Include(p => p.NotificationTriggers)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectNotFoundError<Updated>();

        var mappedTriggers = triggers
            .Select(t => new NotificationTrigger
            {
                ProjectId = projectId,
                Trigger = t.Trigger,
                Threshold = t.Threshold,
                ThresholdUnit = t.ThresholdUnit,
                Action = t.Action,
            })
            .ToList();

        await _notificationScheduler.SetNotificationTriggersForProjectAsync(project.NotificationTriggers, mappedTriggers);

        return new ErrorOr<Updated>();
    }

    private async Task<ErrorOr<Deleted>> RemoveInvitation(string projectId, string inviteeName)
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

    public async Task<ErrorOr<Updated>> ConfigureUserNotificationAsync(
        string userId,
        string projectId,
        List<NotificationKinds> notificationKinds
    )
    {
        var participant = await _dataContext.ProjectParticipants.FindAsync(projectId, userId);
        if (participant == null)
            return Error.NotFound("Project.NotFound", "Project was not found for user.");

        var notificationKindsEnum = NotificationKinds.None;
        foreach (var kind in notificationKinds)
            notificationKindsEnum |= kind;

        participant.EnabledNotificationKinds = notificationKindsEnum;
        _dataContext.ProjectParticipants.Update(participant);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}