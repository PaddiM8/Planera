namespace Planera.Data.Dto;

public class AccountDto
{
    public required string Id { get; init; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public string? AvatarPath { get; set; }

    public InterfaceTheme Theme { get; set; }
}