using OpenIddict.Abstractions;

namespace GeekOAuth.Infrastructure;

public class ScopeSeeder(IOpenIddictScopeManager scopeManager) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
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

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
