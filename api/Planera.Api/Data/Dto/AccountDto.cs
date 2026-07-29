using Planera.Api.Data.Notifications;

namespace Planera.Api.Data.Dto;

public class AccountDto
{
    public required string Id { get; init; }

    public required string Username { get; set; }

    public required string Email { get; set; }
    
    public required bool HasPassword { get; set; }

    public string? AvatarPath { get; set; }

    public InterfaceTheme Theme { get; set; }

    public List<NotificationKinds> EnabledNotificationKinds { get; set; } = [];
}