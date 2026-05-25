using System.ComponentModel.DataAnnotations;
using GeekOAuth.Models;
using GeekOAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GeekOAuth.Pages.Account;

public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    AuditLogger audit) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await signInManager.UserManager.FindByEmailAsync(Input.Email);
            audit.LoginSuccess(user?.Id ?? "unknown", ip, null);
            return LocalRedirect(ReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("TwoFactor", new { returnUrl = ReturnUrl });
        }

        if (result.IsLockedOut)
        {
            var user = await signInManager.UserManager.FindByEmailAsync(Input.Email);
            var lockoutEnd = user is not null
                ? await signInManager.UserManager.GetLockoutEndDateAsync(user)
                : null;
            var remaining = lockoutEnd.HasValue
                ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                : 15;

            ErrorMessage = $"Account locked. Try again in {remaining} minute(s).";
            audit.LoginFailure(Input.Email, ip, "Lockout", 5);
            return Page();
        }

        ErrorMessage = "Invalid email or password.";
        var failCount = 0;
        var failUser = await signInManager.UserManager.FindByEmailAsync(Input.Email);
        if (failUser is not null)
            failCount = await signInManager.UserManager.GetAccessFailedCountAsync(failUser);

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
