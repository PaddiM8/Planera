using Microsoft.AspNetCore.Mvc;
using Planera.Api.Extensions;
using Planera.Api.Models.Notifications;
using Planera.Api.Services;

namespace Planera.Api.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationController(NotificationService notificationService) : ControllerBase
{
    private readonly NotificationService _notificationService = notificationService;

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushNotificationModel pushNotification)
    {
        var result = await _notificationService.SubscribeAsync(
            User.FindFirst("Id")!.Value,
            pushNotification.Endpoint,
            pushNotification.Keys
        );
            
        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshPushNotificationModel refreshPushNotification)
    {
        var result = await _notificationService.RefreshAsync(
            User.FindFirst("Id")!.Value,
            refreshPushNotification.OldEndpoint,
            refreshPushNotification.NewEndpoint,
            refreshPushNotification.Keys
        );
            
        return result.ToActionResult();
    }
}