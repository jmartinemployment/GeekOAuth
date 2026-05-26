using System.Globalization;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GeekOAuth.Server.Infrastructure;

public class ClientSeeder(
    IOpenIddictApplicationManager appManager,
    IConfiguration config,
    IHostApplicationLifetime lifetime) : ApplicationStartedHostedService(lifetime)
{
    private static readonly string M2mAccessTokenLifetime = TimeSpan.FromMinutes(60).ToString("c", CultureInfo.InvariantCulture);

    protected override async Task ExecuteAsync(CancellationToken ct)
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
            ConsentType = ConsentTypes.Implicit,
        }, ct);
    }

    private async Task SeedM2mClient(string clientId, string displayName, string secret, CancellationToken ct)
    {
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = secret,
            ClientType = ClientTypes.Confidential,
            DisplayName = displayName,
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "internal.api",
            },
            Settings =
            {
                [Settings.TokenLifetimes.AccessToken] = M2mAccessTokenLifetime,
            },
        }, ct);
    }

    private Task SeedGeekApiClient(CancellationToken ct)
    {
        var secret = config["CLIENT_SECRET_GEEKAPI"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKAPI is required");
        return SeedM2mClient("geekapi", "GeekAPI (M2M)", secret, ct);
    }

    private Task SeedWebsiteClient(CancellationToken ct)
    {
        var secret = config["CLIENT_SECRET_GEEKWEBSITE"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKWEBSITE is required");
        return SeedM2mClient("geekatyourspot-website", "GeekatYourSpot Website", secret, ct);
    }
}
