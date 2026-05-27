using System.Security.Claims;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

// "AuthenticationTypes.Federation" = "AuthenticationTypes.Federation"
// Use the literal string to avoid adding Microsoft.IdentityModel.Tokens dependency


namespace GeekOAuth.Controllers;

[ApiController]
public class AuthorizationController(
    IOpenIddictApplicationManager appManager,
    IOpenIddictAuthorizationManager authManager,
    IOpenIddictScopeManager scopeManager,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuditLogger audit) : ControllerBase
{
    [EnableRateLimiting("authorize-endpoint")]
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request not found.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            // Browsers need 302 to login, not 401 + Location (Chrome shows "HTTP ERROR 401").
            var returnUrl = $"{Request.PathBase}{Request.Path}{Request.QueryString}";
            return LocalRedirect($"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var user = await userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("User not found.");

        var application = await appManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("Application not found.");

        var authorizations = await authManager.FindAsync(
            subject: await userManager.GetUserIdAsync(user),
            client: await appManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var resolvedConsentType = await appManager.GetConsentTypeAsync(application);
        if (resolvedConsentType != ConsentTypes.Implicit && !authorizations.Any())
        {
            return LocalRedirect($"/Consent?returnUrl={Uri.EscapeDataString(Request.PathBase + Request.Path + Request.QueryString)}");
        }

        var identity = new ClaimsIdentity(
            authenticationType: "AuthenticationTypes.Federation",
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
            .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

        if (!string.IsNullOrEmpty(user.DisplayName))
            identity.SetClaim("display_name", user.DisplayName);

        if (!string.IsNullOrEmpty(user.Tenant))
            identity.SetClaim("tenant", user.Tenant);

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        var authorization = authorizations.LastOrDefault();
        var clientId = await appManager.GetIdAsync(application)
            ?? throw new InvalidOperationException("Application identifier is missing.");

        authorization ??= await authManager.CreateAsync(
            identity: identity,
            subject: await userManager.GetUserIdAsync(user),
            client: clientId,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        audit.TokenIssued("authorization_code", request.ClientId!, user.Id, identity.GetScopes());

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [EnableRateLimiting("token-endpoint")]
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request not found.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var user = await userManager.FindByIdAsync(result.Principal!.GetClaim(Claims.Subject)!);

            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User no longer exists.",
                    }));
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                authenticationType: "AuthenticationTypes.Federation",
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
                .SetClaims(Claims.Role, [.. await userManager.GetRolesAsync(user)]);

            if (!string.IsNullOrEmpty(user.Tenant))
                identity.SetClaim("tenant", user.Tenant);

            identity.SetDestinations(GetDestinations);

            audit.TokenIssued(request.GrantType!, request.ClientId ?? "unknown", user.Id, identity.GetScopes());

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            identity.SetClaim(Claims.Subject, request.ClientId);
            identity.SetClaim(Claims.ClientId, request.ClientId);

            // S2S role claim — resource servers use this for fine-grained authorization
            var application = await appManager.FindByClientIdAsync(request.ClientId!);
            if (application is not null)
            {
                var displayName = await appManager.GetDisplayNameAsync(application);
                identity.SetClaim("svc_role", request.ClientId);
                identity.SetClaim("svc_name", displayName);
            }

            identity.SetScopes(request.GetScopes());
            identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
            identity.SetDestinations(_ => [Destinations.AccessToken]);

            audit.TokenIssued("client_credentials", request.ClientId!, null, identity.GetScopes());

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("Unsupported grant type.");
    }

    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal!;
        var user = await userManager.FindByIdAsync(claimsPrincipal.GetClaim(Claims.Subject)!);

        if (user is null)
            return Challenge(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claims = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [Claims.Subject] = await userManager.GetUserIdAsync(user),
            [Claims.Email] = await userManager.GetEmailAsync(user),
            [Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user),
            [Claims.Name] = await userManager.GetUserNameAsync(user),
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
            claims["display_name"] = user.DisplayName;

        if (!string.IsNullOrEmpty(user.Tenant))
            claims["tenant"] = user.Tenant;

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or Claims.Subject => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Email => claim.Subject?.HasScope(Scopes.Email) == true
                ? [Destinations.AccessToken, Destinations.IdentityToken]
                : [],
            Claims.Role => claim.Subject?.HasScope(Scopes.Roles) == true
                ? [Destinations.AccessToken, Destinations.IdentityToken]
                : [],
            "tenant" or "display_name" => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken],
        };
    }
}
