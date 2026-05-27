using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class ResetPasswordModel(IUserAccountService accounts) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool ResetComplete { get; private set; }

    public IActionResult OnGet(string? email, string? code)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            return RedirectToPage("ForgotPassword");

        Input.Email = email;
        Input.Code = code;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await accounts.ResetPasswordAsync(Input.Email, Input.Code, Input.Password);
        if (!result.Succeeded)
        {
            ErrorMessage = result.Error ?? "Reset failed.";
            return Page();
        }

        ResetComplete = true;
        return Page();
    }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
