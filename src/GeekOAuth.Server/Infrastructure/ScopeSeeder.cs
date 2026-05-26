using OpenIddict.Abstractions;

namespace GeekOAuth.Server.Infrastructure;

public class ScopeSeeder(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime lifetime) : ApplicationStartedHostedService(lifetime)
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        var scopes = new[]
        {
            new OpenIddictScopeDescriptor { Name = "openid",          DisplayName = "OpenID" },
            new OpenIddictScopeDescriptor { Name = "profile",         DisplayName = "Profile" },
            new OpenIddictScopeDescriptor { Name = "email",           DisplayName = "Email address" },
            new OpenIddictScopeDescriptor { Name = "offline_access",  DisplayName = "Offline access (refresh tokens)" },
            new OpenIddictScopeDescriptor { Name = "internal.api",    DisplayName = "Platform internal API" },
            new OpenIddictScopeDescriptor { Name = "devices.manage",  DisplayName = "Manage trusted devices" },
        };

        foreach (var descriptor in scopes)
        {
            if (await scopeManager.FindByNameAsync(descriptor.Name!, ct) is null)
                await scopeManager.CreateAsync(descriptor, ct);
        }
    }
}
