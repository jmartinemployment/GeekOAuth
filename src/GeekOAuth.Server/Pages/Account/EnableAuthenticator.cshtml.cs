using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using GeekOAuth.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Pages.Account;

[Authorize]
[EnableRateLimiting("login-page")]
public class EnableAuthenticatorModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string SharedKey { get; private set; } = string.Empty;
    public string AuthenticatorUri { get; private set; } = string.Empty;
    public string[] RecoveryCodes { get; private set; } = [];
    public bool CodesGenerated { get; private set; }

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToPage("Login");

        await LoadSharedKeyAndQrCodeUriAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return RedirectToPage("Login");

        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            code);

        if (!isValid)
        {
            ErrorMessage = "Invalid verification code.";
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        RecoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ToArray();
        CodesGenerated = true;
        return Page();
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
    {
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey!);
        AuthenticatorUri = GenerateQrCodeUri(user.Email!, unformattedKey!);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
            result.Append(unformattedKey.AsSpan(currentPosition));
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string issuer = "GeekOAuth";
        return $"otpauth://totp/{UrlEncoder.Default.Encode(issuer)}:{UrlEncoder.Default.Encode(email)}?secret={unformattedKey}&issuer={UrlEncoder.Default.Encode(issuer)}&digits=6";
    }

    public class InputModel
    {
        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}
