using System.Globalization;
using OpenIddict.Abstractions;
using Serilog;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GeekOAuth.Server.Infrastructure;

public class ClientSeeder(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    IHostApplicationLifetime lifetime) : ApplicationStartedHostedService(lifetime)
{
    private static readonly string M2mAccessTokenLifetime = TimeSpan.FromMinutes(60).ToString("c", CultureInfo.InvariantCulture);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        await OidcPublicClientSeeds.SeedPublicClientsAsync(scope.ServiceProvider, ct);

        try
        {
            await SeedGeekApiClient(appManager, ct);
            await SeedWebsiteClient(appManager, ct);
            await SeedGeekGtmClient(appManager, ct);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "M2M OAuth clients were not seeded — set CLIENT_SECRET_GEEKAPI, CLIENT_SECRET_GEEKWEBSITE, and optionally CLIENT_SECRET_GEEKGTM");
        }
    }

    private async Task SeedGeekApiClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        var secret = config["CLIENT_SECRET_GEEKAPI"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKAPI is required");
        await SeedM2mClient(appManager, "geekapi", "GeekAPI (M2M)", secret, ct);
    }

    private async Task SeedWebsiteClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        var secret = config["CLIENT_SECRET_GEEKWEBSITE"]
            ?? throw new InvalidOperationException("CLIENT_SECRET_GEEKWEBSITE is required");
        await SeedM2mClient(appManager, "geekatyourspot-website", "GeekatYourSpot Website", secret, ct);
    }

    private async Task SeedGeekGtmClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        var secret = config["CLIENT_SECRET_GEEKGTM"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            Log.Information("CLIENT_SECRET_GEEKGTM not set — skipping geekgtm M2M client seed");
            return;
        }

        await SeedM2mClient(appManager, "geekgtm", "Geek GTM MCP", secret, ct);
    }

    private static async Task SeedM2mClient(
        IOpenIddictApplicationManager appManager,
        string clientId,
        string displayName,
        string secret,
        CancellationToken ct)
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

}
