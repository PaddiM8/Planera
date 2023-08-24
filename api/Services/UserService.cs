using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Data.Files;

namespace Planera.Services;

public class UserService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorage _fileStorage;

    public UserService(
        DataContext dataContext,
        IMapper mapper,
        UserManager<User> userManager,
        IFileStorage fileStorage)
    {
        _dataContext = dataContext;
        _mapper = mapper;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    public async Task<ErrorOr<UserDto>> GetAsync(string userId)
    {
        var user = await _dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<ErrorOr<AccountDto>> GetAccountAsync(string userId)
    {
        var user = await _dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        return _mapper.Map<AccountDto>(user);
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        string username,
        string email,
        string? avatar)
    {
        var user = await _dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        var existingByName = await _userManager.FindByNameAsync(username);
        if (existingByName != null && existingByName.Id != userId)
            return Error.Conflict("Username.Taken", "Another user with the given username already exists.");

        var previousAvatarPath = user.AvatarPath;
        if (avatar?.StartsWith("data:") is true)
        {
            // Expected format of avatar: `data:image/png;base64,BASE64STRING==`
            var bytes = Convert.FromBase64String(avatar.Split(",")[1]);
            var avatar256 = ImagePreparer.Resize(bytes, 256, 256);
            var avatar32 = ImagePreparer.Resize(bytes, 32, 32);
            user.AvatarPath = await _fileStorage.WriteManyAsync(
                "avatars",
                (avatar256, "256"),
                (avatar32, "32")
            );
        }
        else if (avatar == "")
        {
            user.AvatarPath = null;
        }

        user.UserName = username;
        user.Email = email;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            // If it didn't update, remove the newly created files, since
            // they won't be used.
            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                _fileStorage.Delete(user.AvatarPath, "32");
                _fileStorage.Delete(user.AvatarPath, "256");
            }

            return Error.Unexpected("Unknown", "Failed to update user.");
        }

        if (previousAvatarPath != null && previousAvatarPath != user.AvatarPath)
        {
            _fileStorage.Delete(previousAvatarPath, "32");
            _fileStorage.Delete(previousAvatarPath, "256");
        }

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Updated>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var identityResult = await _userManager.ChangePasswordAsync(user!, currentPassword, newPassword);

        return !identityResult.Succeeded
            ? Error.Validation("CurrentPassword.Invalid", "Failed to change password.")
            : new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<IEnumerable<ProjectDto>>> GetInvitations(string userId)
    {
        return await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Select(x => x.Project)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<InvitationDto>> AcceptInvitation(string userId, string projectId)
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

    public async Task<ErrorOr<Updated>> DeclineInvitation(string userId, string projectId)
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