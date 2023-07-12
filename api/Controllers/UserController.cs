using Microsoft.AspNetCore.Mvc;
using Planera.Data.Dto;
using Planera.Extensions;
using Planera.Services;

namespace Planera.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("invitations")]
    [ProducesResponseType(typeof(IEnumerable<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations()
    {
        var result = await _userService.GetInvitations(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpPost("invitations/{projectId}/accept")]
    public async Task<IActionResult> AcceptInvitation(int projectId)
    {
        var result = await _userService.AcceptInvitation(
            User.FindFirst("Id")!.Value, projectId
        );

        return result.ToActionResult();
    }

    [HttpPost("invitations/{projectId}/decline")]
    public async Task<IActionResult> DeclineInvitation(int projectId)
    {
        var result = await _userService.DeclineInvitation(
            User.FindFirst("Id")!.Value, projectId
        );

        return result.ToActionResult();
    }
}