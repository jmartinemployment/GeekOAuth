using GeekOAuth.Abstractions.Models;
using GeekOAuth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Server.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddGeekIdentity(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 4;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddRoles<IdentityRole>()
        .AddSignInManager()
        .AddDefaultTokenProviders()
        .AddUserStore<DapperUserStore>();

        var isDevelopment = environment.IsDevelopment();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = isDevelopment ? "geek-auth-dev" : "__Host-geek-auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = isDevelopment
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/Login";
            });

        services.Configure<PasswordHasherOptions>(options =>
        {
            options.IterationCount = 600_000;
        });

        return services;
    }
}
