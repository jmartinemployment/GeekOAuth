using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace GeekOAuth.Pages;

public class ConsentModel(IOpenIddictApplicationManager appManager) : PageModel
{
    public string ApplicationName { get; set; } = string.Empty;
    public IList<string> Scopes { get; set; } = [];
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request?.ClientId is not null)
        {
            var app = await appManager.FindByClientIdAsync(request.ClientId);
            if (app is not null)
                ApplicationName = await appManager.GetDisplayNameAsync(app) ?? request.ClientId;

            Scopes = request.GetScopes()
                .Where(s => s is not "openid" and not "offline_access")
                .ToList();
        }

        return Page();
    }

    public IActionResult OnPost(string consent, string? returnUrl = null)
    {
        if (consent == "deny")
        {
            return Forbid(OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }
}
