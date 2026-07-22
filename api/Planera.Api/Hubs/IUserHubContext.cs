using Planera.Api.Data;
using Planera.Api.Data.Dto;
using Planera.Api.Models.Ticket;

namespace Planera.Api.Hubs;

public interface IUserHubContext
{
    public Task OnAddProject(ProjectDto project);

    public Task OnAddInvitation(ProjectDto project);

    public Task SetTheme(InterfaceTheme theme);

    public Task RevokePersonalAccessToken();

    public Task<TicketQueryResult> QueryTickets(
        int startIndex,
        int amount,
        string? query = null,
        TicketSorting sorting = TicketSorting.Newest,
        TicketFilter? filter = null
    );
}
