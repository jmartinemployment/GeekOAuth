using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GeekOAuth.Pages.Account;

public class ConfirmEmailModel(IUserAccountService accounts) : PageModel
{
    public string? Message { get; private set; }
    public bool Succeeded { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            Message = "Invalid confirmation link.";
            return Page();
        }

        var result = await accounts.ConfirmEmailAsync(userId, code);
        Succeeded = result.Succeeded;
        Message = result.Succeeded
            ? "Your email is confirmed. You can sign in now."
            : result.Error ?? "Confirmation failed.";

        return Page();
    }
}
