using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;
using Planera.Data.Files;

namespace Planera.Services;

public class UserService(
    DataContext dataContext,
    IMapper mapper,
    UserManager<User> userManager,
    IFileStorage fileStorage)
{
    public async Task<ErrorOr<UserDto>> GetAsync(string userId)
    {
        var user = await dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        return mapper.Map<UserDto>(user);
    }

    public async Task<ErrorOr<AccountDto>> GetAccountAsync(string userId)
    {
        var user = await dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        return mapper.Map<AccountDto>(user);
    }

    public async Task<ErrorOr<Updated>> EditAsync(
        string userId,
        string? username,
        string? email,
        string? avatar,
        InterfaceTheme? theme)
    {
        var user = await dataContext.Users.FindAsync(userId);
        if (user == null)
            return Error.NotFound("UserId.NotFound", "A user with the given ID was not found.");

        if (username != null)
        {
            var existingByName = await userManager.FindByNameAsync(username);
            if (existingByName != null && existingByName.Id != userId)
                return Error.Conflict("Username.Taken", "Another user with the given username already exists.");
        }

        var previousAvatarPath = user.AvatarPath;
        if (avatar?.StartsWith("data:") is true)
        {
            // Expected format of avatar: `data:image/png;base64,BASE64STRING==`
            var bytes = Convert.FromBase64String(avatar.Split(",")[1]);
            var avatar256 = ImagePreparer.Resize(bytes, 256, 256);
            var avatar32 = ImagePreparer.Resize(bytes, 32, 32);
            user.AvatarPath = await fileStorage.WriteManyAsync(
                "avatars",
                (avatar256, "256"),
                (avatar32, "32")
            );
        }
        else if (avatar == "")
        {
            user.AvatarPath = null;
        }

        if (username != null)
            user.UserName = username;

        if (email != null)
            user.Email = email;

        if (theme != null)
            user.Theme = theme.Value;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            // If it didn't update, remove the newly created files, since
            // they won't be used.
            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                fileStorage.Delete(user.AvatarPath, "32");
                fileStorage.Delete(user.AvatarPath, "256");
            }

            return Error.Unexpected("Unknown", "Failed to update user.");
        }

        if (previousAvatarPath != null && previousAvatarPath != user.AvatarPath)
        {
            fileStorage.Delete(previousAvatarPath, "32");
            fileStorage.Delete(previousAvatarPath, "256");
        }

        return new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<Updated>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        var identityResult = await userManager.ChangePasswordAsync(user!, currentPassword, newPassword);

        return !identityResult.Succeeded
            ? Error.Validation("CurrentPassword.Invalid", "Incorrect password.")
            : new ErrorOr<Updated>();
    }

    public async Task<ErrorOr<IEnumerable<ProjectDto>>> GetInvitations(string userId)
    {
        return await dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Select(x => x.Project)
            .ProjectTo<ProjectDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ErrorOr<InvitationDto>> AcceptInvitation(string userId, string projectId)
    {
        var invitation = await dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Include(x => x.User)
            .Where(x => x.ProjectId == projectId)
            .Include(x => x.Project)
            .ThenInclude(x => x.Author)
            .SingleOrDefaultAsync();
        if (invitation == null)
            return Error.NotFound("Invitation.NotFound", "Invitation was not found.");

        await dataContext.ProjectParticipants.AddAsync(new ProjectParticipant
        {
            UserId = userId,
            ProjectId = projectId,
        });
        dataContext.Invitations.Remove(invitation);
        await dataContext.SaveChangesAsync();

        return mapper.Map<InvitationDto>(invitation);
    }

    public async Task<ErrorOr<Updated>> DeclineInvitation(string userId, string projectId)
    {
        var invitation = await dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Where(x => x.ProjectId == projectId)
            .SingleOrDefaultAsync();
        if (invitation == null)
            return Error.NotFound("Invitation.NotFound", "Invitation was not found.");

        dataContext.Invitations.Remove(invitation);
        await dataContext.SaveChangesAsync();

        return new ErrorOr<Updated>();
    }
}