using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace GeekOAuth.Controllers;

public sealed record ProvisionTenantOwnerRequest(
    [Required] Guid TenantId,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    string? DisplayName);

/// <summary>
/// Creates the login of a Geek-Receptionist tenant's owner (docs/Geek-Receptionist.md section 7, signup step 4). Only
/// GeekAPI may call it: the token must be a client-credentials token of the geekapi client, because this authority is
/// shared across Geek apps and the other service clients also hold internal.api. The tenant is written once, here, and
/// nothing else in GeekOAuth sets or changes it, so the tenant_id claim on the owner's tokens cannot be edited by the owner.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[EnableRateLimiting("token-endpoint")]
public sealed class TenantOwnersController(IUserAccountService accounts) : ControllerBase
{
    internal const string RequiredClient = "geekapi";
    internal const string RequiredScope = "internal.api";

    [HttpPost("api/internal/tenant-owners")]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantOwnerRequest request, CancellationToken ct)
    {
        if (!IsGeekApi(User))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Only GeekAPI may create a tenant owner." });

        if (!ModelState.IsValid)
            return BadRequest(new { error = "TenantId, Email and a Password of at least 8 characters are required." });

        var result = await accounts.ProvisionTenantOwnerAsync(request.TenantId, request.Email, request.Password, request.DisplayName, ct);
        if (result.Succeeded)
            return Created(string.Empty, new { userId = result.UserId, tenantId = request.TenantId });

        return result.Conflict
            ? Conflict(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }

    internal static bool IsGeekApi(System.Security.Claims.ClaimsPrincipal user) =>
        string.Equals(user.FindFirst("svc_role")?.Value, RequiredClient, StringComparison.Ordinal)
        && user.HasScope(RequiredScope);
}
