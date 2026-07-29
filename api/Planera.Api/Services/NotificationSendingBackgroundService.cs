using System.Net;
using Newtonsoft.Json;
using Planera.Api.Data;
using Planera.Api.Data.Notifications;
using Planera.Api.Data.Projects;
using Planera.Api.Models.Notifications;
using WebPush;

namespace Planera.Api.Services;

public class NotificationSendingBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    VapidDetails vapidDetails,
    IHttpClientFactory httpClientFactory,
    JsonSerializerSettings jsonSerializerSettings,
    ILogger<NotificationSendingBackgroundService> logger
) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly VapidDetails _vapidDetails = vapidDetails;
    private readonly JsonSerializerSettings _jsonSerializerSettings = jsonSerializerSettings;
    private readonly ILogger<NotificationSendingBackgroundService> _logger = logger;
    private readonly HttpClient _httpClient =  httpClientFactory.CreateClient();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await SendNotificationsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notifications");
            }

            await timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    private async Task SendNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
        var now = DateTime.UtcNow;

        var entriesToRemove = new List<NotificationQueueEntry>();
        var itemsToSend = dataContext
            .NotificationQueue
            .Where(x => x.ScheduledTime <= now)
            .ToList();
        foreach (var batch in itemsToSend.Chunk(100))
        {
            var tasks = batch
                .Select(entry => SendAsync(notificationService, entry, cancellationToken))
                .ToList();
            await Task.WhenAll(tasks);

            var successfulEntries = tasks
                .Where(t => t.Result.success)
                .Select(t => t.Result.entry);
            entriesToRemove.AddRange(successfulEntries!);
        }

        dataContext.NotificationQueue.RemoveRange(entriesToRemove);
        await dataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(NotificationQueueEntry? entry, bool success)> SendAsync(
        NotificationService notificationService,
        NotificationQueueEntry entry,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (entry.ActionKind == NotificationActionKind.PushNotification)
            {
                return await SendPushNotificationAsync(notificationService, entry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when sending notification for queue entry for NotificationTrigger {NotificationTriggerId}", entry.NotificationTriggerId);
        }

        return (null, false);
    }

    private async Task<(NotificationQueueEntry? entry, bool success)> SendPushNotificationAsync(
        NotificationService notificationService,
        NotificationQueueEntry entry,
        CancellationToken cancellationToken
    )
    {
        var webPushClient = new WebPushClient(_httpClient);

        var subscriptionsResult = await notificationService.GetSubscriptionsForUsersInProjectAsync(entry.TargetId, entry.NotificationKind);
        if (subscriptionsResult.IsError)
            return (null, false);

        // As long as it managed to send it to one of the subscriptions, that is good enough.
        bool success = true;
        foreach (var (subscription, participation) in subscriptionsResult.Value)
        {
            var userExpectsAllNotifications = entry.NotificationKind == NotificationKinds.DeadlineMyTicket &&
                participation.EnabledNotificationKinds.HasFlag(NotificationKinds.DeadlineOtherTicket);
            if (entry.AssignedUserIds?.Contains(subscription.UserId) is false && !userExpectsAllNotifications)
                continue;

            var remoteSubscription = new PushSubscription(subscription.Endpoint, subscription.P256Dh, subscription.Auth);

            try
            {
                var payload = new PushNotificationPayload
                {
                    Title = entry.Title,
                    Content = entry.Content,
                    Data = new PushNotificationData
                    {
                        Url = entry.Url ?? "/",
                    },
                };
                var payloadString = JsonConvert.SerializeObject(payload, _jsonSerializerSettings);

                await webPushClient.SendNotificationAsync(remoteSubscription, payloadString, _vapidDetails, cancellationToken);
                success = true;
            }
            catch (WebPushException ex)
            {
                // If the subscription is no longer active, remove it from the database and consider it to be
                // a success since it was handled.
                if (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
                {
                    await notificationService.UnsubscribeAsync(subscription);
                    success = true;
                }
            }
        }

        return (entry, success);
    }
}