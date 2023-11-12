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

    private async Task<ErrorOr<IQueryable<Ticket>>> QueryAsync(string userId, string projectId, int ticketId)
    {
        var project = await _projectService
            .QueryById(userId, projectId)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<IQueryable<Ticket>>();

        return ErrorOrFactory.From(
            _dataContext.Tickets
                .Where(x => x.Id == ticketId && x.ProjectId == projectId)
        );
    }

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
            .Include(x => x.Notes.OrderBy(note => note.Timestamp))
            .SingleOrDefaultAsync();

        return ticket == null
            ? TicketNotFoundError<TicketDto>()
            : _mapper.Map<TicketDto>(ticket);
    }

    // TODO: Return an object containing both a collection of tickets and the
    // sorting/filtering options that were used.
    public async Task<ErrorOr<TicketQueryResult>> GetAllAsync(
        string userId,
        string username,
        string slug,
        int startIndex,
        int amount,
        string? searchQuery = null,
        TicketSorting? sorting = null,
        TicketFilter? filter = null)
    {
        var project = await _projectService
            .QueryBySlug(userId, username, slug)
            .SingleOrDefaultAsync();
        if (project == null)
            return ProjectService.ProjectNotFoundError<TicketQueryResult>();

        // If sorting was provided explicitly, the sorting/filter should be saved
        // for the user-project combination. Otherwise, the existing saved values
        // should be used.
        var projectParticipant = await _dataContext.ProjectParticipants.FindAsync(project.Id, userId);
        if (sorting == null)
        {
            sorting = projectParticipant!.Sorting;
            filter = projectParticipant.Filter;
        }
        else if (projectParticipant!.Sorting != sorting.Value || projectParticipant.Filter != filter)
        {
            projectParticipant.Sorting = sorting.Value;
            projectParticipant.Filter = filter;
            _dataContext.Update(projectParticipant);
            await _dataContext.SaveChangesAsync();
        }

        var query = _dataContext.Tickets
            .Where(x => x.ProjectId == project.Id);

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query
                .Select(x =>
                    // Mostly rely on TS vector full text search
                    new
                    {
                        Row = x,
                        Similarity = (double)EF.Functions
                            .ToTsVector("english", x.Title + " " + x.Description)
                            .Rank(EF.Functions.PhraseToTsQuery(searchQuery)) * 100,
                    }
                )
                .Select(x =>
                    // But also do a trigram word search to eg. find partial searches of words
                    new
                    {
                        Row = x.Row,
                        Similarity = x.Similarity + EF.Functions
                            .TrigramsWordSimilarity(searchQuery, x.Row.Title),
                    }
                )
                .Where(x => x.Similarity > 0.5)
                .OrderByDescending(x => x.Similarity)
                .Select(x => x.Row);
        }

        if (filter != null)
        {
            query = filter switch
            {
                TicketFilter.Open => query.Where(x => x.Status == TicketStatus.None),
                TicketFilter.Closed => query.Where(x => x.Status == TicketStatus.Closed),
                TicketFilter.Inactive => query.Where(x => x.Status == TicketStatus.Inactive),
                TicketFilter.Done => query.Where(x => x.Status == TicketStatus.Done),
                TicketFilter.AssignedToMe => query.Where(x =>
                    (x.Status == TicketStatus.None || x.Status == TicketStatus.Inactive)
                        && x.Assignees.Any(user => user.Id == userId)
                ),
                _ => query,
            };
        }

        if (string.IsNullOrEmpty(searchQuery))
        {
            query = sorting switch
            {
                TicketSorting.Oldest => query.OrderBy(x => x.Timestamp),
                TicketSorting.HighestPriority => query.OrderByDescending(x => x.Priority),
                TicketSorting.LowestPriority => query.OrderBy(x => x.Priority),
                _ => query.OrderByDescending(x => x.Timestamp),
            };
        }

        var tickets = await query
            .Include(x => x.Author)
            .Include(x => x.Assignees)
            .Skip(startIndex)
            .Take(amount)
            .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new TicketQueryResult(tickets, sorting.Value, filter);
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

    public async Task<ErrorOr<ICollection<UserDto>>> AddAssigneeAsync(
        string userId,
        string projectId,
        int ticketId,
        string assigneeId)
    {
        var query = await QueryAsync(userId, projectId, ticketId);
        if (query.IsError)
            return query.Errors;

        var ticket = await query.Value
            .Include(x => x.Assignees)
            .SingleOrDefaultAsync();
        if (ticket == null)
            return TicketNotFoundError<ICollection<UserDto>>();

        var assignee = await _dataContext.Users.FindAsync(assigneeId);
        if (assignee == null)
            return Error.NotFound("AssigneeId.NotFound", "A user with the given ID was not found.");

        await _dataContext.TicketAssignees.AddAsync(
            new TicketAssignee
            {
                Ticket = ticket,
                User = assignee,
            }
        );
        await _dataContext.SaveChangesAsync();

        return ErrorOrFactory.From(
            _mapper.Map<ICollection<UserDto>>(ticket.Assignees)
        );
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
