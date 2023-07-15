using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Services;

public class UserService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public UserService(
        DataContext dataContext,
        IMapper mapper,
        UserManager<User> userManager)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ErrorOr<AccountDto>> GetAsync(string userId)
    {
        var user = await _dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        return _mapper.Map<AccountDto>(user);
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        string username,
        string email)
    {
        var user = await _dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        var existingByName = await _userManager.FindByNameAsync(username);
        if (existingByName != null && existingByName.Id != userId)
            return Error.Conflict("Username.Taken", "Another user with the given username already exists.");

        user.UserName = username;
        user.Email = email;
        await _userManager.UpdateAsync(user);

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<IEnumerable<ProjectDto>>> GetInvitations(string userId)
    {
        return await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Select(x => x.Project)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<InvitationDto>> AcceptInvitation(string userId, int projectId)
    {
        var invitation = await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Include(x => x.User)
            .Where(x => x.ProjectId == projectId)
            .Include(x => x.Project)
            .ThenInclude(x => x.Author)
            .SingleOrDefaultAsync();
        if (invitation == null)
            return Error.NotFound("Invitation.NotFound", "Invitation was not found.");

        await _dataContext.ProjectParticipants.AddAsync(new ProjectParticipant
        {
            UserId = userId,
            ProjectId = projectId,
        });
        _dataContext.Invitations.Remove(invitation);
        await _dataContext.SaveChangesAsync();

        return _mapper.Map<InvitationDto>(invitation);
    }

    public async Task<ErrorOr<Updated>> DeclineInvitation(string userId, int projectId)
    {
        var invitation = await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Where(x => x.ProjectId == projectId)
            .SingleOrDefaultAsync();
        if (invitation == null)
            return Error.NotFound("Invitation.NotFound", "Invitation was not found.");

        _dataContext.Invitations.Remove(invitation);
        await _dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}