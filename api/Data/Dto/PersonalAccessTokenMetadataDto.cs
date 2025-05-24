namespace Planera.Data.Dto;

public class PersonalAccessTokenMetadataDto
{
    public required string UserId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
