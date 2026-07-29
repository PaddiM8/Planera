namespace Planera.Api.Data.Notifications;

[Flags]
public enum NotificationKinds
{
    None = 0,
    Core = 1 << 0,
    DeadlineMyTicket = 1 << 1,
    DeadlineOtherTicket = 1 << 2,
}