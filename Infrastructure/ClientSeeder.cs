using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GeekOAuth.Infrastructure;

public class ClientSeeder(IOpenIddictApplicationManager appManager, IConfiguration config) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await SeedElectronClient(ct);
        await SeedGeekApiClient(ct);
        await SeedWebsiteClient(ct);
    }

    private async Task SeedElectronClient(CancellationToken ct)
    {
        const string clientId = "geek-seo-electron";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek SEO Electron",
            RedirectUris = { new Uri("http://127.0.0.1") },
            PostLogoutRedirectUris = { new Uri("http://127.0.0.1") },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + "devices.manage",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
        }, ct);
    }

    private async Task SeedGeekApiClient(CancellationToken ct)
    {
        const string clientId = "geekapi";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        var secret = config["CLIENT_SECRET_GEEKAPI"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKAPI is required");

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = secret,
            ClientType = ClientTypes.Confidential,
            DisplayName = "GeekAPI (M2M)",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "internal.api",
            },
        }, ct);
    }

    private async Task SeedWebsiteClient(CancellationToken ct)
    {
        const string clientId = "geekatyourspot-website";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        var secret = config["CLIENT_SECRET_GEEKWEBSITE"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKWEBSITE is required");

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = secret,
            ClientType = ClientTypes.Confidential,
            DisplayName = "GeekatYourSpot Website",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "internal.api",
            },
        }, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
