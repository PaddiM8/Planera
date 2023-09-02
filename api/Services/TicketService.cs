using System.Collections;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Planera.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Planera.Data.Dto;
using Planera.Data.Files;
using Planera.Models.Ticket;

namespace Planera.Services;

public class TicketService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly ProjectService _projectService;
    private readonly IFileStorage _fileStorage;

    private static readonly Regex _imageSrcRegex = new(
        "(\\<img[^>]+src=)['\"]([^'\"]+)['\"]",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    public TicketService(
        DataContext dataContext,
        IMapper mapper,
        ProjectService projectService,
        IFileStorage fileStorage)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _projectService = projectService;
        _fileStorage = fileStorage;
    }

    public static ErrorOr<T> TicketNotFoundError<T>()
        => Error.Conflict("Ticket.NotFound", "Ticket was not found.");

    private async Task<ErrorOr<Ticket>> FindAsync(string userId, string projectId, int ticketId)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<Ticket>();

        var ticket = await _dataContext.Tickets.FindAsync(ticketId, projectId);

        return ticket ?? TicketNotFoundError<Ticket>();
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
            .Include(x => x.Project)
            .Include(x => x.Assignees)
            .Include(x => x.Notes)
            .SingleOrDefaultAsync();

        return ticket == null
            ? TicketNotFoundError<TicketDto>()
            : _mapper.Map<TicketDto>(ticket);
    }

    public async Task<ErrorOr<IEnumerable<TicketDto>>> GetAllAsync(
        string userId,
        string username,
        string slug,
        int startIndex,
        int amount,
        string? searchQuery = null,
        TicketSorting sorting = TicketSorting.Newest,
        TicketStatus? filterByStatus = null)
    {
        var project = await _projectService
            .QueryBySlug(userId, username, slug)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<IEnumerable<TicketDto>>();

        var query = _dataContext.Tickets
            .Where(x => x.ProjectId == project.Id);

        if (searchQuery != null)
            query = query.Where(x => x.Title.Contains(searchQuery));

        if (filterByStatus != null)
            query = query.Where(x => x.Status == filterByStatus);

        query = sorting switch
        {
            TicketSorting.Newest => query.OrderByDescending(x => x.Timestamp),
            TicketSorting.Oldest => query.OrderBy(x => x.Timestamp),
            TicketSorting.HighestPriority => query.OrderByDescending(x => x.Priority),
            TicketSorting.LowestPriority => query.OrderBy(x => x.Priority),
            _ => throw new ArgumentException("sorting"),
        };

        return await query
            .Include(x => x.Author)
            .Include(x => x.Assignees)
            .Skip(startIndex)
            .Take(amount)
            .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<TicketDto>> AddTicketAsync(
        string userId,
        string projectId,
        string title,
        string description,
        TicketPriority priority,
        IEnumerable<string> assigneeIds)
    {
        await using var transaction = await _dataContext.Database.BeginTransactionAsync();

        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<TicketDto>();

        var lastTicket = await _dataContext.Tickets
            .Where(x => x.ProjectId == project.Id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
        int id = lastTicket?.Id + 1 ?? 1;
        var assignees = await _dataContext.Users
            .Where(x => assigneeIds.Contains(x.Id))
            .ToListAsync();
        var ticket = new Ticket
        {
            Id = id,
            ProjectId = project.Id,
            Title = title,
            Description = await SaveImagesAndReplaceUrls(project.Id, description),
            Priority = priority,
            Assignees = assignees,
            AuthorId = userId,
            Timestamp = DateTime.UtcNow,
        };
        await _dataContext.Tickets.AddAsync(ticket);
        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<ErrorOr<Updated>> EditTicketAsync(
        string userId,
        string projectId,
        int ticketId,
        string title,
        string description)
    {
        var ticketResult = await FindAsync(userId, projectId, ticketId);
        if (ticketResult.IsError)
            return ticketResult.Errors;

        var ticket = ticketResult.Value;
        ticket.Title = title;
        ticket.Description = await SaveImagesAndReplaceUrls(
            projectId,
            description,
            ticket.Description
        );
        _dataContext.Tickets.Update(ticket);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Deleted>> RemoveTicketAsync(
        string userId,
        string projectId,
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
        string projectId,
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
        string projectId,
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
        string projectId,
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
        string projectId,
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

    private async Task<string> SaveImagesAndReplaceUrls(string projectId, string ticketBody, string? oldBody = null)
    {
        // If the pre-edit body of the ticket was provided, put all the
        // existing image paths in a collection and remove the ones that
        // exist in the new body as well. In the end, we will end up
        // with a collection of all the images that were removed.
        // These should be removed from the file storage.
        HashSet<string>? filesToRemove = null;
        const string urlPrefix = "planera:";
        if (oldBody != null)
        {
            filesToRemove = new HashSet<string>(
                _imageSrcRegex.Matches(oldBody)
                    .Where(x => x.Groups.Count > 2)
                    .Select(x => x.Groups[2].Value)
                    .Where(x => x.StartsWith(urlPrefix))
            );
            Console.WriteLine("Files to remove: " + string.Join(", ", filesToRemove.ToArray()));
        }

        var writeTasks = new List<Task<string>>();
        foreach (Match match in _imageSrcRegex.Matches(ticketBody))
        {
            if (match.Groups.Count < 3)
                continue;

            var src = match.Groups[2].Value;

            // If an already saved image was both in the previous body and the
            // new one, remove it from the to-be-removed list.
            if (filesToRemove != null && src.StartsWith(urlPrefix) && filesToRemove.Contains(src))
                filesToRemove.Remove(src);

            // Only base64 encoded png images should be saved.
            if (!src.StartsWith("data:image/png;base64,"))
                continue;

            var bytes = Convert.FromBase64String(src.Split(",")[1]);
            var encoded = ImagePreparer.Encode(bytes);
            writeTasks.Add(_fileStorage.WriteAsync(projectId, encoded));
        }

        if (filesToRemove != null)
        {
            foreach (var fileToRemove in filesToRemove.Select(x => x[urlPrefix.Length..]))
                _fileStorage.Delete(fileToRemove);
        }

        var newFileNames = new Queue(await Task.WhenAll(writeTasks.ToArray()));

        // Perform an identical second pass, but this time replace all the values
        return _imageSrcRegex.Replace(
            ticketBody,
            matches =>
            {
                if (matches.Groups.Count < 3 || !matches.Groups[2].Value.StartsWith("data:image/png;base64,"))
                    return matches.Value;

                var newFileName = newFileNames.Dequeue()!;

                return $"{matches.Groups[1]}'{urlPrefix}{newFileName}'";
            }
        );
    }
}