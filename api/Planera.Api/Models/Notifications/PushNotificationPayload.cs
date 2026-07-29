namespace Planera.Api.Models.Notifications;

public class PushNotificationPayload
{
    public required string Title { get; set; }

    public required string Body { get; set; }
    
    public PushNotificationData? Data { get; set; }
}

public class PushNotificationData
{
    public string? Url { get; set; }
}