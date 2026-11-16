using GeekOAuth.Abstractions.Security;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;

namespace GeekOAuth.Pages.Account;

public class ConfirmEmailModel(IUserAccountService accounts) : PageModel
{
    public string? Message { get; private set; }
    public bool Succeeded { get; private set; }

    /// <summary>The authorize request the person started signing up from, when the link carries one that is acceptable.</summary>
    public string? ReturnUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code, string? returnUrl = null)
    {
        if (AuthorizeReturnUrl.IsValid(returnUrl))
            ReturnUrl = returnUrl;
        else if (!string.IsNullOrEmpty(returnUrl))
            Log.Warning("ConfirmEmail ignored a returnUrl that is not an authorize request. Len={Len}", returnUrl.Length);

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
