using Planera.Api.Data;
using Planera.Api.Data.Dto;

namespace Planera.Api.Hubs;

public interface IUserHubContext
{
    public Task OnAddProject(ProjectDto project);

    public Task OnAddInvitation(ProjectDto project);

    public Task SetTheme(InterfaceTheme theme);

    public Task RevokePersonalAccessToken();
}
