namespace GeekOAuth.Server.Infrastructure;

internal static class OidcClientBootstrap
{
    public static Task SeedPublicClientsAsync(IServiceProvider services, CancellationToken ct = default) =>
        OidcPublicClientSeeds.SeedPublicClientsAsync(services, ct);
}
