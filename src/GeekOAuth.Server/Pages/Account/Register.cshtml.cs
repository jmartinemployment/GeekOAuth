using System.ComponentModel.DataAnnotations;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace GeekOAuth.Pages.Account;

[EnableRateLimiting("login-page")]
public class RegisterModel(IUserAccountService accounts) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool RegistrationSent { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await accounts.RegisterAsync(Input.Email, Input.Password);
        if (!result.Succeeded)
        {
            ErrorMessage = result.Error ?? "Registration failed.";
            return Page();
        }

        RegistrationSent = true;
        return Page();
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
