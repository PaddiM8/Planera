using Planera.Data.Dto;

namespace Planera.Models.Authentication;

public record AuthenticationResult(string Token, UserDto User);