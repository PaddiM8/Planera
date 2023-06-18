using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Planera.Data;

namespace Planera.Services;

public class ProjectService
{
    private readonly DataContext _dataContext;

    public ProjectService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<ErrorOr<ICollection<Project>>> GetAllAsync(string authorName)
    {
        var user = await _dataContext.Users
            .Include(x => x.Projects)
            .SingleOrDefaultAsync(x => x.UserName == authorName);

        return user == null
            ? Error.NotFound("Id.NotFound", "A user with that id does not exist.")
            : ErrorOrFactory.From(user.Projects);
    }

    public async Task<ErrorOr<Project>> GetAsync(string authorName, string slug)
    {
        var project = await _dataContext.Projects
            .Where(x => x.Author.UserName == authorName && x.Slug == slug)
            .SingleOrDefaultAsync();

        if (project == null)
            return Error.NotFound("Slug.NotFound", "A project with the given slug was not found.");

        return project;
    }

    public async Task<ErrorOr<Created>> AddAsync(string authorId, string slug, string name)
    {
        if (await _dataContext.Projects.AnyAsync(x => x.Slug == slug))
            return Error.Conflict("Slug.AlreadyExists", "A project with the given slug already exists.");

        await _dataContext.Projects.AddAsync(new Project
        {
            Name = name,
            Slug = slug,
            AuthorId = authorId,
        });
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Created>();
    }

    public async Task<ErrorOr<Updated>> EditAsync(string authorName, string slug, string name)
    {
        var result = await GetAsync(authorName, slug);
        if (result.IsError)
            return result.Errors;

        var project = result.Value;
        project.Name = name;

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
}