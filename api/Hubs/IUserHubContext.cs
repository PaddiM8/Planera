using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Hubs;

public interface IUserHubContext
{
    public Task OnAddProject(ProjectDto project);

    public Task OnAddInvitation(ProjectDto project);

    public Task SetTheme(InterfaceTheme theme);
}