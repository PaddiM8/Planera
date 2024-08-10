using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Planera.Data;
using Planera.Extensions;
using Planera.Services;

namespace Planera.Hubs;

[Authorize]
public class UserHub(UserService userService, IHubContext<ProjectHub, IProjectHubContext> projectHub)
    : Hub<IUserHubContext>
{
    public async Task AcceptInvitation(string projectId)
    {
        var userId = Context.User!.FindFirst("Id")!.Value;
        var result = await userService.AcceptInvitation(userId, projectId);

        var invitation = result.Unwrap();
        await Clients
            .User(Context.User!.Identity!.Name!)
            .OnAddProject(invitation.Project);
        await projectHub.Clients
            .Group(projectId)
            .OnAddParticipant(invitation.User);
    }

    public async Task DeclineInvitation(string projectId)
    {
        var userId = Context.User!.FindFirst("Id")!.Value;
        var result = await userService.DeclineInvitation(userId, projectId);
        result.Unwrap();
    }

    public async Task SetTheme(InterfaceTheme theme)
    {
        var userId = Context.User!.FindFirst("Id")!.Value;
        var result = await userService.EditAsync(
            userId,
            null,
            null,
            null,
            theme
        );
        result.Unwrap();
    }
}