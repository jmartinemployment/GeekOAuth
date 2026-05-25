using GeekOAuth.Models;
using GeekOAuth.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddGeekIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddSignInManager()
        .AddDefaultTokenProviders()
        .AddUserStore<DapperUserStore>();

        return services;
    }
}
