using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class ForgotPasswordModel(IUserAccountService accounts) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool EmailSent { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await accounts.RequestPasswordResetAsync(Input.Email);
        EmailSent = true;
        return Page();
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
