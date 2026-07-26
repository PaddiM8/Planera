namespace Planera.Api.Models.Notifications;

public class RefreshPushNotificationModel
{
    public required string OldEndpoint { get; set; }
    
    public required string NewEndpoint { get; set; }
    
    public DateTimeOffset? ExpirationTime { get; set; }
    
    public required NotificationSubscriptionKeys Keys { get; set; }
}