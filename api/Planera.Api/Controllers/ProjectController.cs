using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Planera.Api.Data.Dto;
using Planera.Api.Hubs;
using Planera.Api.Models.Project;
using Planera.Api.Services;
using Planera.Api.Extensions;

namespace Planera.Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectController(
    ProjectService projectService,
    IHubContext<UserHub, IUserHubContext> userHub,
    IHubContext<ProjectHub, IProjectHubContext> projectHub)
    : ControllerBase
{
    private readonly ProjectService _projectService = projectService;
    private readonly IHubContext<UserHub, IUserHubContext> _userHub = userHub;
    private readonly IHubContext<ProjectHub, IProjectHubContext> _projectHub = projectHub;

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _projectService.GetAllAsync(User.Identity!.Name!);

        return result.ToActionResult();
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ICollection<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFromUser(string username)
    {
        // TODO: Allow viewing public projects when that's a thing
        if (username != User.Identity?.Name)
            return Unauthorized();

        var result = await _projectService.GetAllAsync(username);

        return result.ToActionResult();
    }

    [HttpGet("{username}/{slug}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string username, string slug)
    {
        var result = await _projectService.GetAsync(
            User.FindFirst("Id")!.Value,
            username,
            slug
        );

        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateProjectModel model) {
        var result = await _projectService.AddAsync(
            User.FindFirst("Id")!.Value,
            model.Slug,
            model.Name,
            model.Description,
            model.Icon
        );

        return result.ToActionResult();
    }

    [HttpPut("{username}/{slug}")]
    public async Task<IActionResult> Edit(
        string username,
        string slug,
        [FromBody] EditProjectModel model)
    {
        var result = await _projectService.EditAsync(
            User.FindFirst("Id")!.Value,
            username,
            slug,
            model.Name,
            model.Description,
            model.Icon,
            model.EnableTicketDescriptions,
            model.EnableTicketAssignees,
            model.EnableTicketDeadlines
        );

        return result.ToActionResult();
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> Remove(string projectId)
    {
        var result = await _projectService.RemoveAsync(
            User.FindFirst("Id")!.Value,
            projectId
        );

        return result.ToActionResult();
    }

    [HttpPut("{projectId}/inviteParticipant")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> InviteParticipant(string projectId, string participantName)
    {
        var result = await _projectService.InviteParticipantAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            participantName
        );
        await _userHub.Clients
            .User(participantName)
            .OnAddInvitation(result.Value.project);

        return result.ToActionResult();
    }

    [HttpDelete("{projectId}/removeParticipant")]
    public async Task<IActionResult> RemoveParticipant(string projectId, string participantName)
    {
        var result = await _projectService.RemoveParticipantAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            participantName
        );
        await _projectHub.Clients
            .Group(projectId.ToString())
            .OnRemoveParticipant(participantName);

        return result.ToActionResult();
    }
}
