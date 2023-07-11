using Microsoft.AspNetCore.Mvc;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Models;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("projects")]
public class ProjectController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ICollection<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string username)
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

    [HttpPost("{username}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        string username,
        [FromBody] CreateProjectModel model)
    {
        if (username != User.Identity?.Name)
            return Unauthorized();

        var result = await _projectService.AddAsync(
            User.FindFirst("Id")!.Value,
            model.Slug,
            model.Name,
            model.Description
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
            model.Description
        );

        return result.ToActionResult();
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> Remove(int projectId)
    {
        var result = await _projectService.RemoveAsync(
            User.FindFirst("Id")!.Value,
            projectId
        );

        return result.ToActionResult();
    }

    [HttpGet("{projectId}/tickets")]
    [ProducesResponseType(typeof(ICollection<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets(int projectId)
    {
        var result = await _projectService.GetTicketsAsync(
            User.FindFirst("Id")!.Value,
            projectId
        );

        return result.ToActionResult();
    }

    [HttpPut("{projectId}/addParticipant")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddParticipant(int projectId, string participantName)
    {
        var result = await _projectService.AddParticipantAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            participantName
        );

        return result.ToActionResult();
    }

    [HttpDelete("{projectId}/removeParticipant")]
    public async Task<IActionResult> RemoveParticipant(int projectId, string participantName)
    {
        var result = await _projectService.RemoveParticipantAsync(
            User.FindFirst("Id")!.Value,
            projectId,
            participantName
        );

        return result.ToActionResult();
    }
}