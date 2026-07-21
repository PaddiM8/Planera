namespace Planera.Api.Models;

public class OidcOptions
{
    public string? ProviderId { get; set; }
    
    public string? ProviderName { get; set; }
    
    public string? ProviderUrl { get; set; }
    
    public string? ProviderIconUrl { get; set; }
    
    public string? ClientId { get; set; }
    
    public string? ClientSecret { get; set; }
    
    public string? Scopes { get; set; }
    
    public string? CallbackPath { get; set; }

    public bool RequireVerifiedEmail { get; set; } = true;
}