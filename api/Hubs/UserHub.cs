using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Planera.Extensions;
using Planera.Services;

namespace Planera.Hubs;

[Authorize]
public class UserHub : Hub<IUserHubContext>
{
    private readonly UserService _userService;
    private readonly IHubContext<ProjectHub, IProjectHubContext> _projectHub;

    public UserHub(UserService userService, IHubContext<ProjectHub, IProjectHubContext> projectHub)
    {
        _userService = userService;
        _projectHub = projectHub;
    }

    public async Task AcceptInvitation(string projectId)
    {
        string userId = Context.User!.FindFirst("Id")!.Value;
        var result = await _userService.AcceptInvitation(userId, projectId);

        var invitation = result.Unwrap();
        await Clients
            .User(Context.User!.Identity!.Name!)
            .OnAddProject(invitation.Project);
        await _projectHub.Clients
            .Group(projectId)
            .OnAddParticipant(invitation.User);
    }

    public async Task DeclineInvitation(string projectId)
    {
        string userId = Context.User!.FindFirst("Id")!.Value;
        var result = await _userService.DeclineInvitation(userId, projectId);
        result.Unwrap();
    }
}