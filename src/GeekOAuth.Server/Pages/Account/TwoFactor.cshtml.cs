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
    // "Keep me signed in" from the password step; the 2FA step issues the cookie, so it has to carry the choice.
    [BindProperty] public bool RememberMe { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null, bool rememberMe = false)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToPage("Login");

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToPage("Login");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var spaceless = Input.Code.Replace(" ", string.Empty);
        var digits = spaceless.Replace("-", string.Empty);

        // The two kinds of code never overlap: an authenticator code is exactly six digits, and a
        // recovery code is longer. The input's shape picks the path; a failed attempt never falls through to the other.
        var isAuthenticatorCode = digits.Length == 6 && digits.All(char.IsAsciiDigit);

        var result = isAuthenticatorCode
            ? await signInManager.TwoFactorAuthenticatorSignInAsync(
                digits, isPersistent: RememberMe, rememberClient: Input.RememberDevice)
            : await signInManager.TwoFactorRecoveryCodeSignInAsync(spaceless);

        if (result.Succeeded)
        {
            audit.TwoFactorSuccess(user.Id, ip);
            return LocalRedirect(ReturnUrl ?? Url.Content("~/"));
        }

        audit.TwoFactorFailure(user.Id, ip);
        ErrorMessage = "Invalid authenticator or recovery code.";
        return Page();
    }

    public class InputModel
    {
        // Up to 32 so a pasted "123 456" or an Identity recovery code is not truncated or rejected before the server normalises it.
        [Required, StringLength(32)]
        public string Code { get; set; } = string.Empty;

        public bool RememberDevice { get; set; }
    }
}
