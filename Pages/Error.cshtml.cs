using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace GeekOAuth.Pages;

public class ErrorModel : PageModel
{
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }

    public void OnGet()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        Error = response?.Error;
        ErrorDescription = response?.ErrorDescription;
    }
}
