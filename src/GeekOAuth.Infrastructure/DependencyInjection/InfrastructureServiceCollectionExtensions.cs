using Dapper;
using GeekOAuth.Abstractions.Data;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Repositories;
using GeekOAuth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GeekOAuth.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    static InfrastructureServiceCollectionExtensions() =>
        DefaultTypeMap.MatchNamesWithUnderscores = true;

    public static IServiceCollection AddGeekOAuthInfrastructure(
        this IServiceCollection services,
        NpgsqlDataSource dataSource)
    {
        services.AddSingleton(dataSource);
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(dataSource));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserStore<ApplicationUser>, DapperUserStore>();
        services.AddScoped<IRoleStore<IdentityRole>, NoOpRoleStore>();

        return services;
    }
}
