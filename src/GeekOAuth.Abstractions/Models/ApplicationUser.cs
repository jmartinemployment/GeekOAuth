using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Abstractions.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Tenant { get; set; }
}
