using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Security;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class LoginModel(
    IUserAuthenticationService authentication,
    UserManager<ApplicationUser> userManager,
    IAuditLogger audit) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var attempt = await authentication.SignInAsync(Input.Email, Input.Password, Input.RememberMe);

        if (attempt.Result.Succeeded)
        {
            audit.LoginSuccess(attempt.User?.Id ?? "unknown", ip, null);
            return LocalRedirect(ReturnUrl);
        }

        if (attempt.Result.RequiresTwoFactor)
            return RedirectToPage("TwoFactor", new { returnUrl = ReturnUrl });

        if (attempt.Result.IsLockedOut)
        {
            var remaining = 15;
            if (attempt.User is not null)
            {
                var lockoutEnd = await userManager.GetLockoutEndDateAsync(attempt.User);
                if (lockoutEnd.HasValue)
                    remaining = (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes);
            }

            ErrorMessage = $"Account locked. Try again in {remaining} minute(s).";
            audit.LoginFailure(Input.Email, ip, "Lockout", 5);
            return Page();
        }

        if (attempt.Result.IsNotAllowed)
        {
            ErrorMessage = "Confirm your email before signing in. Check your inbox for the confirmation link, or register again if SMTP is not set up yet.";
            audit.LoginFailure(Input.Email, ip, "NotAllowed", 0);
            return Page();
        }

        ErrorMessage = "Invalid email or password.";
        var failCount = 0;
        if (attempt.User is not null)
            failCount = await userManager.GetAccessFailedCountAsync(attempt.User);

        audit.LoginFailure(Input.Email, ip, "InvalidCredentials", failCount);
        return Page();
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
