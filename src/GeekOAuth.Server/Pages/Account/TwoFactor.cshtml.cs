using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class TwoFactorModel(
    SignInManager<ApplicationUser> signInManager,
    IAuditLogger audit) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToPage("Login");

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToPage("Login");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            code, isPersistent: false, rememberClient: Input.RememberDevice);

        if (result.Succeeded)
        {
            audit.TwoFactorSuccess(user.Id, ip);
            return LocalRedirect(ReturnUrl ?? Url.Content("~/"));
        }

        audit.TwoFactorFailure(user.Id, ip);
        ErrorMessage = "Invalid authenticator code.";
        return Page();
    }

    public class InputModel
    {
        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        public bool RememberDevice { get; set; }
    }
}
