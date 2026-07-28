using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace GeekOAuth.Server.Infrastructure;

/// <summary>
/// Allows Content Writer V3 Vercel preview deployments to use per-branch redirect URIs
/// (e.g. content-writer-v3-git-main-….vercel.app) without registering each one.
/// </summary>
public sealed partial class ContentWriterAwareApplicationManager(
    IOpenIddictApplicationCache<OpenIddictEntityFrameworkCoreApplication> cache,
    ILogger<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>> logger,
    IOptionsMonitor<OpenIddictCoreOptions> options,
    IOpenIddictApplicationStore<OpenIddictEntityFrameworkCoreApplication> store)
    : OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>(cache, logger, options, store)
{
    public override async ValueTask<bool> ValidateRedirectUriAsync(
        OpenIddictEntityFrameworkCoreApplication application,
        string uri,
        CancellationToken cancellationToken = default)
    {
        if (await base.ValidateRedirectUriAsync(application, uri, cancellationToken))
            return true;

        var clientId = await GetClientIdAsync(application, cancellationToken);
        return clientId == "content-writer-v3" && IsAllowedVercelCallback(uri);
    }

    public override async ValueTask<bool> ValidatePostLogoutRedirectUriAsync(
        OpenIddictEntityFrameworkCoreApplication application,
        string uri,
        CancellationToken cancellationToken = default)
    {
        if (await base.ValidatePostLogoutRedirectUriAsync(application, uri, cancellationToken))
            return true;

        var clientId = await GetClientIdAsync(application, cancellationToken);
        return clientId == "content-writer-v3" && IsAllowedVercelPostLogout(uri);
    }

    private static bool IsAllowedVercelCallback(string uri) =>
        ContentWriterVercelCallback().IsMatch(uri);

    private static bool IsAllowedVercelPostLogout(string uri) =>
        ContentWriterVercelPostLogout().IsMatch(uri);

    // Production: content-writer-v3.vercel.app
    // Previews: content-writer-v3-git-main-….vercel.app / content-writer-v3-<hash>-….vercel.app
    [GeneratedRegex(@"^https://content-writer-v3([\w.-]+)?\.vercel\.app/auth/callback$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ContentWriterVercelCallback();

    [GeneratedRegex(@"^https://content-writer-v3([\w.-]+)?\.vercel\.app/?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ContentWriterVercelPostLogout();
}
