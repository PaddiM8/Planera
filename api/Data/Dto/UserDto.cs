namespace Planera.Data.Dto;

public class UserDto
{
    public required string Id { get; init; }

    public required string Username { get; init; }

    public string? AvatarPath { get; init; }
}