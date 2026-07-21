namespace Planera.Api.Models.Authentication;

public class AuthenticationInfo
{
    public bool PasswordAuthenticationDisabled { get; set; }
    
    public bool RegistrationDisabled { get; set; }
    
    public OidcAuthentiationInfo? Oidc { get; set; }
}

public class OidcAuthentiationInfo
{
    public required string ProviderId { get; set; }
    
    public required string ProviderName { get; set; }
    
    public string? ProviderIconUrl { get; set; }
    
    public bool RequireVerifiedEmail { get; set; }
}