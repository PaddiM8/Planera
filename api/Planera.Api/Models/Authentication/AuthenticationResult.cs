using Planera.Api.Data.Dto;

namespace Planera.Api.Models.Authentication;

public record AuthenticationResult(string Token, UserDto User);