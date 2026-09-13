using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace GeekOAuth.Server;

/// <summary>
/// Configures <see cref="ForwardedHeadersOptions"/> with an explicit trusted-proxy list.
/// Never clears KnownIPNetworks/KnownProxies unless at least one trusted hop is configured.
/// </summary>
public static class ForwardedHeadersConfiguration
{
    public const string TrustedProxyCidrsKey = "TRUSTED_PROXY_CIDRS";
    public const string TrustedProxyIpsKey = "TRUSTED_PROXY_IPS";

    public static void Apply(
        ForwardedHeadersOptions options,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _ = environment;
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        var cidrs = SplitCsv(configuration[TrustedProxyCidrsKey]);
        var ips = SplitCsv(configuration[TrustedProxyIpsKey]);

        if (cidrs.Count == 0 && ips.Count == 0)
        {
            // Keep ASP.NET defaults (loopback). Do not trust arbitrary clients as proxies.
            return;
        }

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var cidr in cidrs)
        {
            if (TryParseCidr(cidr, out var network))
                options.KnownIPNetworks.Add(network);
        }

        foreach (var ipText in ips)
        {
            if (IPAddress.TryParse(ipText, out var ip))
                options.KnownProxies.Add(ip);
        }
    }

    private static List<string> SplitCsv(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(s => s.Length > 0)
                .ToList();

    private static bool TryParseCidr(string cidr, out System.Net.IPNetwork network)
    {
        network = default!;
        var parts = cidr.Split('/', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return false;
        if (!IPAddress.TryParse(parts[0], out var prefix))
            return false;
        if (!int.TryParse(parts[1], out var prefixLength))
            return false;

        try
        {
            network = new System.Net.IPNetwork(prefix, prefixLength);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
