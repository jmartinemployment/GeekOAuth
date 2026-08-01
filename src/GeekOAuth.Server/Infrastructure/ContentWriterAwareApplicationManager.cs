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
        return IsPreviewAwareClient(clientId) && IsAllowedVercelCallback(clientId, uri);
    }

    public override async ValueTask<bool> ValidatePostLogoutRedirectUriAsync(
        OpenIddictEntityFrameworkCoreApplication application,
        string uri,
        CancellationToken cancellationToken = default)
    {
        if (await base.ValidatePostLogoutRedirectUriAsync(application, uri, cancellationToken))
            return true;

        var clientId = await GetClientIdAsync(application, cancellationToken);
        return IsPreviewAwareClient(clientId) && IsAllowedVercelPostLogout(clientId, uri);
    }

    private static bool IsPreviewAwareClient(string? clientId) =>
        clientId is "content-writer-v3" or "geek-content-workflow" or "geek-content-creator";

    private static bool IsAllowedVercelCallback(string? clientId, string uri) =>
        clientId switch
        {
            "content-writer-v3" => ContentWriterVercelCallback().IsMatch(uri),
            "geek-content-workflow" => GeekContentWorkflowVercelCallback().IsMatch(uri),
            "geek-content-creator" => GeekContentCreatorVercelCallback().IsMatch(uri),
            _ => false,
        };

    private static bool IsAllowedVercelPostLogout(string? clientId, string uri) =>
        clientId switch
        {
            "content-writer-v3" => ContentWriterVercelPostLogout().IsMatch(uri),
            "geek-content-workflow" => GeekContentWorkflowVercelPostLogout().IsMatch(uri),
            "geek-content-creator" => GeekContentCreatorVercelPostLogout().IsMatch(uri),
            _ => false,
        };

    // Production: content-writer-v3.vercel.app
    // Previews: content-writer-v3-git-main-….vercel.app / content-writer-v3-<hash>-….vercel.app
    [GeneratedRegex(@"^https://content-writer-v3([\w.-]+)?\.vercel\.app/auth/callback$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ContentWriterVercelCallback();

    [GeneratedRegex(@"^https://content-writer-v3([\w.-]+)?\.vercel\.app/?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ContentWriterVercelPostLogout();

    [GeneratedRegex(@"^https://geek-content-workflow([\w.-]+)?\.vercel\.app/auth/callback$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GeekContentWorkflowVercelCallback();

    [GeneratedRegex(@"^https://geek-content-workflow([\w.-]+)?\.vercel\.app/?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GeekContentWorkflowVercelPostLogout();

    [GeneratedRegex(@"^https://geek-content-creator([\w.-]+)?\.vercel\.app/auth/callback$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GeekContentCreatorVercelCallback();

    [GeneratedRegex(@"^https://geek-content-creator([\w.-]+)?\.vercel\.app/?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GeekContentCreatorVercelPostLogout();
}
