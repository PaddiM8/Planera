using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Planera.Api.Data.Dto;
using Planera.Api.Data.Notifications;
using Planera.Api.Models.User;
using Planera.Api.Services;
using Planera.Api.Extensions;

namespace Planera.Api.Controllers;

[ApiController]
[Route("user")]
public class UserController(
    UserService userService,
    PersonalAccessTokenService personalAccessTokenService,
    IConfiguration configuration
) : ControllerBase
{
    private readonly UserService _userService = userService;
    private readonly PersonalAccessTokenService _personalAccessTokenService = personalAccessTokenService;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _userService.GetAsync(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpGet("account")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccount()
    {
        var result = await _userService.GetAccountAsync(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Edit([FromBody] EditUserModel model)
    {
        var result = await _userService.EditAsync(
            User.FindFirst("Id")!.Value,
            model.Username,
            model.Email,
            model.Avatar,
            model.Theme
        );

        return result.ToActionResult();
    }

    [HttpPut("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        var result = await _userService.ChangePasswordAsync(
            User.FindFirst("Id")!.Value,
            model.CurrentPassword,
            model.NewPassword
        );

        return result.ToActionResult();
    }

    [HttpPut("configureNotifications")]
    public async Task<IActionResult> ConfigureNotifications(List<NotificationKinds> notificationKinds)
    {
        var result = await _userService.ConfigureNotificationAsync(
            User.FindFirstValue("Id")!,
            notificationKinds
        );

        return result.ToActionResult();
    }

    [HttpGet("pinnedProjects")]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPinnedProjects()
    {
        var result = await _userService.GetPinnedProjectsAsync(User.FindFirstValue("Id")!);

        return result.ToActionResult();
    }

    [HttpPut("pinnedProjects")]
    public async Task<IActionResult> SetPinnedProjects([FromBody] List<string> pinnedProjectIds)
    {
        var result = await _userService.SetPinnedProjectsAsync(User.FindFirstValue("Id")!, pinnedProjectIds);

        return result.ToActionResult();
    }

    [HttpGet("invitations")]
    [ProducesResponseType(typeof(IEnumerable<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations()
    {
        var result = await _userService.GetInvitations(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpPost("invitations/{projectId}/accept")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptInvitation(string projectId)
    {
        var result = await _userService.AcceptInvitation(
            User.FindFirst("Id")!.Value,
            projectId
        );

        return result.ToActionResult();
    }

    [HttpGet("invitations/{projectId}/accept")]
    public async Task<IActionResult> AcceptInvitationAndRedirect(string projectId)
    {
        var result = await _userService.AcceptInvitation(
            User.FindFirst("Id")!.Value,
            projectId
        );
        if (result.IsError)
            return Redirect($"{_configuration["FrontendUrl"]}/invitations");

        return Redirect($"{_configuration["FrontendUrl"]}/projects/{result.Value.Project.Author.Username}/{result.Value.Project.Slug}");
    }

    [HttpPost("invitations/{projectId}/decline")]
    public async Task<IActionResult> DeclineInvitation(string projectId)
    {
        var result = await _userService.DeclineInvitation(
            User.FindFirst("Id")!.Value,
            projectId
        );

        return result.ToActionResult();
    }

    [HttpGet("invitations/{projectId}/decline")]
    public async Task<IActionResult> DeclineInvitationAndRedirect(string projectId)
    {
        // Just redirect to the invitation page to add an extra confirmation step
        return Redirect($"{_configuration["FrontendUrl"]}/invitations");
    }

    [HttpGet("tokens/personal-access-token")]
    [ProducesResponseType(typeof(PersonalAccessTokenMetadataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonalAccessTokenMetadata()
    {
        var result = await _personalAccessTokenService.GetMetadata(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpPost("tokens/personal-access-token")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePersonalAccessToken()
    {
        var result = await _personalAccessTokenService.CreateAsync(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }

    [HttpDelete("tokens/personal-access-token")]
    public async Task<IActionResult> RevokePersonalAccessToken()
    {
        var result = await _personalAccessTokenService.RevokeAsync(User.FindFirst("Id")!.Value);

        return result.ToActionResult();
    }
}
