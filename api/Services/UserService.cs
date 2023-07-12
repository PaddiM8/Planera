using AutoMapper;
using AutoMapper.QueryableExtensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Planera.Data;
using Planera.Data.Dto;

namespace Planera.Services;

public class UserService
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public UserService(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public async Task<ErrorOr<IEnumerable<ProjectDto>>> GetInvitations(string userId)
    {
        var projects = await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Select(x => x.Project)
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return projects;
    }

    public async Task<ErrorOr<Updated>> AcceptInvitation(string userId, int projectId)
    {
        var invitation = await _dataContext.Invitations
            .Where(x => x.UserId == userId)
            .Where(x => x.ProjectId == projectId)
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

        return new ErrorOr<Updated>();
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