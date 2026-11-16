using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Security;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class RegisterModel(IUserAccountService accounts) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    /// <summary>The authorize request an app sent the person here from; kept so they land back in that app after they sign in.</summary>
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RegistrationSent { get; private set; }
    public bool AutoConfirmed { get; private set; }

    public void OnGet() => KeepOnlyAuthorizeReturnUrl();

    public async Task<IActionResult> OnPostAsync()
    {
        KeepOnlyAuthorizeReturnUrl();
        if (!ModelState.IsValid) return Page();

        var result = await accounts.RegisterAsync(Input.Email, Input.Password, ReturnUrl);
        if (!result.Succeeded)
        {
            ErrorMessage = result.Error ?? "Registration failed.";
            return Page();
        }

        RegistrationSent = true;
        AutoConfirmed = result.AutoConfirmed;
        return Page();
    }

    private void KeepOnlyAuthorizeReturnUrl()
    {
        if (string.IsNullOrEmpty(ReturnUrl) || AuthorizeReturnUrl.IsValid(ReturnUrl)) return;
        Log.Warning("Register ignored a ReturnUrl that is not an authorize request. Len={Len}", ReturnUrl.Length);
        ReturnUrl = null;
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
