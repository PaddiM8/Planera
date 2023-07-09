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

    public async Task<ErrorOr<ICollection<ProjectDto>>> GetAllAsync(string authorName)
    {
        var user = await _dataContext.Users
            .Include(x => x.Projects)
            .SingleOrDefaultAsync(x => x.UserName == authorName);

        if (user == null)
            return Error.NotFound("Id.NotFound", "A user with that id does not exist.");
        return ErrorOrFactory.From(
            _mapper.Map<ICollection<Project>, ICollection<ProjectDto>>(user.Projects)
        );
    }

    public async Task<ErrorOr<ProjectDto>> GetAsync(string authorName, string slug)
    {
        var project = await _dataContext.Projects
            .Where(x => x.Author.UserName == authorName && x.Slug == slug)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        if (project == null)
            return Error.NotFound("Slug.NotFound", "A project with the given slug was not found.");

        return project;
    }

    public async Task<ErrorOr<int>> AddAsync(string authorId, string slug, string name, string description)
    {
        if (await _dataContext.Projects.AnyAsync(x => x.Slug == slug))
            return Error.Conflict("Slug.AlreadyExists", "A project with the given slug already exists.");

        var project = new Project
        {
            AuthorId = authorId,
            Slug = slug,
            Name = name,
            Description = description,
        };
        await _dataContext.Projects.AddAsync(project);
        await _dataContext.SaveChangesAsync();

        return project.Id;
    }

    public async Task<ErrorOr<Updated>> EditAsync(string authorName, string slug, string name,string description)
    {
        var project = await _dataContext.Projects
            .Where(x => x.Author.UserName == authorName && x.Slug == slug)
            .SingleOrDefaultAsync();

        if (project == null)
            return Error.NotFound("Slug.NotFound", "A project with the given slug was not found.");

        project.Name = name;
        project.Description = description;

        _dataContext.Projects.Update(project);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Deleted>> RemoveAsync(string authorName, string slug)
    {
        var project = await _dataContext.Projects
            .Where(x => x.Author.UserName == authorName && x.Slug == slug)
            .SingleOrDefaultAsync();

        if (project == null)
            return Error.NotFound("Slug.NotFound", "A project with the given slug was not found.");

        _dataContext.Projects.Remove(project);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Deleted>();
    }

    public async Task<ErrorOr<ICollection<TicketDto>>> GetTicketsAsync(string projectAuthor, string projectSlug)
    {
        var project = await _dataContext.Projects
            .Where(x => x.Author.UserName == projectAuthor && x.Slug == projectSlug)
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Author)
            .Include(x => x.Tickets)
            .ThenInclude(x => x.Assignees)
            .SingleOrDefaultAsync();

        return project == null
            ? Error.NotFound("Slug.NotFound", "A project with the given slug was not found.")
            : ErrorOrFactory.From(_mapper.Map<ICollection<TicketDto>>(project.Tickets));
    }
}