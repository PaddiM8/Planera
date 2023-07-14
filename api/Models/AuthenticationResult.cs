using Planera.Data.Dto;

namespace Planera.Models;

public record AuthenticationResult(string Token, UserDto User);