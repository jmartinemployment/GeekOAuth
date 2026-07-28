using OpenIddict.Abstractions;
using Serilog;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GeekOAuth.Server.Infrastructure;

/// <summary>
/// Public PKCE clients required for Geek SEO web and Electron — seeded at startup before listening.
/// </summary>
internal static class OidcPublicClientSeeds
{
    public static async Task SeedPublicClientsAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        await SeedElectronClient(appManager, ct);
        await SeedGeekSeoWebClient(appManager, ct);
        await SeedGeekAdminClient(appManager, ct);
        await SeedContentWriterV3Client(appManager, ct);
    }

    private static async Task SeedElectronClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
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
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekSeoWebClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geekseo";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek SEO (Web)",
            RedirectUris =
            {
                new Uri("https://seo.geekatyourspot.com/auth/callback"),
                new Uri("http://localhost:3000/auth/callback"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://seo.geekatyourspot.com/"),
                new Uri("http://localhost:3000/"),
            },
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
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekAdminClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-admin";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Blog Admin",
            RedirectUris =
            {
                new Uri("https://admin.geekatyourspot.com/auth/callback"),
                new Uri("http://localhost:3002/auth/callback"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://admin.geekatyourspot.com/"),
                new Uri("http://localhost:3002/"),
            },
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
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedContentWriterV3Client(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "content-writer-v3";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Content Writer V3",
            RedirectUris =
            {
                new Uri("https://content-writer.geekatyourspot.com/login"),
                new Uri("http://localhost:3000/login"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://content-writer.geekatyourspot.com/"),
                new Uri("http://localhost:3000/"),
            },
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
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }
}
