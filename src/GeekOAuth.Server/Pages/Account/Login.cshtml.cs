using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Security;
using GeekOAuth.Abstractions.Services;
using GeekOAuth.Server.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class LoginModel(
    IUserAuthenticationService authentication,
    UserManager<ApplicationUser> userManager,
    IAuditLogger audit) : PageModel
{
    private const string ReturnUrlTempDataKey = "LoginReturnUrl";

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public IActionResult OnGet(string? message = null)
    {
        if (message == "signed-in")
            SuccessMessage = "You are signed in. Return to your application and click Sign in again to continue.";

        // Persist across POST so password managers / stripped hidden fields cannot drop the OAuth return URL.
        if (!string.IsNullOrEmpty(ReturnUrl))
            TempData[ReturnUrlTempDataKey] = ReturnUrl;
        else if (TempData.Peek(ReturnUrlTempDataKey) is string saved && !string.IsNullOrEmpty(saved))
            ReturnUrl = saved;

        // #region agent log
        AgentDebugLog.Write("H1", "Login.OnGet", "login GET", new
        {
            hasReturnUrl = !string.IsNullOrEmpty(ReturnUrl),
            returnUrlLen = ReturnUrl?.Length ?? 0,
            returnUrlPrefix = ReturnUrl is { Length: > 0 } ? ReturnUrl[..Math.Min(48, ReturnUrl.Length)] : null,
            message,
            authenticated = User.Identity?.IsAuthenticated == true,
        });
        // #endregion
        Log.Information("Login GET ReturnUrlPresent={Present} Len={Len} Authenticated={Auth}",
            !string.IsNullOrEmpty(ReturnUrl), ReturnUrl?.Length ?? 0, User.Identity?.IsAuthenticated == true);

        if (!string.IsNullOrEmpty(ReturnUrl) && User.Identity?.IsAuthenticated == true)
            return LocalRedirect(ReturnUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(ReturnUrl) && TempData[ReturnUrlTempDataKey] is string saved)
            ReturnUrl = saved;
        else if (!string.IsNullOrEmpty(ReturnUrl))
            TempData[ReturnUrlTempDataKey] = ReturnUrl;

        if (!ModelState.IsValid) return Page();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var attempt = await authentication.SignInAsync(Input.Email, Input.Password, Input.RememberMe);

        if (attempt.Result.Succeeded)
        {
            audit.LoginSuccess(attempt.User?.Id ?? "unknown", ip, null);
            var isLocal = !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl);
            // #region agent log
            AgentDebugLog.Write("H2", "Login.OnPost", "login success redirect decision", new
            {
                hasReturnUrl = !string.IsNullOrEmpty(ReturnUrl),
                returnUrlLen = ReturnUrl?.Length ?? 0,
                returnUrlPrefix = ReturnUrl is { Length: > 0 } ? ReturnUrl[..Math.Min(48, ReturnUrl.Length)] : null,
                isLocal,
                willContinueOAuth = isLocal,
            });
            // #endregion
            Log.Information("Login success ReturnUrlPresent={Present} IsLocal={IsLocal} Len={Len}",
                !string.IsNullOrEmpty(ReturnUrl), isLocal, ReturnUrl?.Length ?? 0);

            if (isLocal)
            {
                TempData.Remove(ReturnUrlTempDataKey);
                return LocalRedirect(ReturnUrl!);
            }

            return RedirectToPage("/Account/Login", new { message = "signed-in" });
        }

        // Keep ReturnUrl available after a failed attempt re-render.
        if (!string.IsNullOrEmpty(ReturnUrl))
            TempData[ReturnUrlTempDataKey] = ReturnUrl;

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
