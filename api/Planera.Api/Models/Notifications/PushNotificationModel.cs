namespace Planera.Api.Models.Notifications;

public class PushNotificationModel
{
    public required string Endpoint { get; set; }
    
    public DateTimeOffset? ExpirationTime { get; set; }
    
    public required NotificationSubscriptionKeys Keys { get; set; }
}

public class NotificationSubscriptionKeys
{
    public required string P256Dh { get; set; }
    
    public required string Auth { get; set; }
}