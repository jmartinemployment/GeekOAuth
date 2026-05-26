using GeekOAuth.Abstractions.Security;
using GeekOAuth.Abstractions.Services;
using GeekOAuth.Application.Security;
using GeekOAuth.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeekOAuth.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddGeekOAuthApplication(this IServiceCollection services)
    {
        services.AddSingleton<TimingSafePasswordVerifier>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddSingleton<IAuthEmailService, AuthEmailService>();
        services.AddSingleton<IAuditLogger, AuditLogger>();

        return services;
    }
}
