using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Planera.Api.Data;
using Planera.Api.Models;
using Planera.Api.Services;

namespace Planera.Api;

public static class OpenIdEventHandler
{
    public static async Task OnTokenValidatedAsync(TokenValidatedContext context, bool disableRegistration)
    {
        var oidcOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<OidcOptions>>();
        var claims = context.Principal?.Claims.ToList();
        var isEmailVerifiedClaim = claims?.FirstOrDefault(c => c.Type == "email_verified")?.Value;
        if (oidcOptions.Value.RequireVerifiedEmail && (!bool.TryParse(isEmailVerifiedClaim, out var isVerified) || !isVerified))
        {
            context.Fail("Your email address must be verified in the authentication authority before signing in.");
            return;
        }

        var username = claims?.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
        var sub = claims?.FirstOrDefault(c => c.Type == "sub")?.Value;
        var avatarUrl = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;
        if (email == null)
        {
            context.Fail("Missing email address.");
            return;
        }
        
        if (sub == null)
        {
            context.Fail("Missing sub.");
            return;
        }

        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
        var providerId = oidcOptions.Value.ProviderId!;
        var providerName = oidcOptions.Value.ProviderName;
        var existingLogin = await userManager.FindByLoginAsync(providerId, sub);
        if (existingLogin != null)
        {
            return;
        }

        var existingUserByEmail = await userManager.FindByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            var userLoginInfo = new UserLoginInfo(providerId, sub, providerName);
            var linkResult = await userManager.AddLoginAsync(existingUserByEmail, userLoginInfo);
            if (!linkResult.Succeeded)
                context.Fail("Failed to link OIDC provider to account.");

            return;
        }

        if (disableRegistration)
        {
            context.Fail("User registration is disabled.");
            return;
        }
        
        if (username == null)
        {
            context.Fail("Missing username address.");
            return;
        }
        
        var authenticationService = context.HttpContext.RequestServices.GetRequiredService<PlaneraAuthenticationService>();
        var registrationSucceeded = await authenticationService.RegisterOidcAsync(username, email, sub, avatarUrl);
        if (!registrationSucceeded)
            context.Fail("Registration failed.");
    }

    public static async Task OnTicketReceivedAsync(TicketReceivedContext context)
    {
        var principal = context.Principal;
        var sub = principal?.FindFirst("sub")?.Value ?? string.Empty;
        var username = principal?.FindFirst("preferred_username")?.Value ?? string.Empty;
        var email = principal?.FindFirst("email")?.Value ?? string.Empty;

        var authenticationService = context.HttpContext.RequestServices.GetRequiredService<PlaneraAuthenticationService>();
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
        var oidcOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<OidcOptions>>();
        var user = await userManager.FindByLoginAsync(oidcOptions.Value.ProviderId!, sub);
        if (user == null)
        {
            context.Fail("Failed to generate token. OIDC user not found.");
            return;
        }

        var jwt = authenticationService.GenerateToken(user.Id, username, email);
        context.Response.Cookies.Append("token", jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
        });

        context.HandleResponse();
        context.Response.Redirect("/");
    }

    public static Task OnRedirectToIdentityProvider(RedirectContext context, string callbackPath)
    {
        var uri = new Uri(context.ProtocolMessage.RedirectUri);
        context.ProtocolMessage.RedirectUri = uri.GetLeftPart(UriPartial.Authority) + callbackPath;
                    
        return Task.FromResult(0);
    }
}