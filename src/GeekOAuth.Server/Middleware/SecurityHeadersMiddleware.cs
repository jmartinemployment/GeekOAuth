namespace GeekOAuth.Server.Middleware;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        if (!environment.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        headers["X-Frame-Options"] = "DENY";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Content-Security-Policy"] = BuildContentSecurityPolicy(context);
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        await next(context);
    }

    private string BuildContentSecurityPolicy(HttpContext context)
    {
        // Chrome validates the entire redirect chain after a form POST. Login posts to
        // /Account/Login, then redirects through /connect/authorize to the client's
        // redirect_uri. Those cross-origin hops must be listed in form-action.
        var formActionOrigins = ResolveFormActionOrigins(context);
        var formAction = formActionOrigins.Length > 0
            ? $"form-action 'self' {string.Join(' ', formActionOrigins)}"
            : "form-action 'self'";

        return string.Join("; ",
            "default-src 'self'",
            "script-src 'self'",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data:",
            "frame-ancestors 'none'",
            "base-uri 'self'",
            formAction);
    }

    private string[] ResolveFormActionOrigins(HttpContext context)
    {
        var raw = configuration["FORM_ACTION_ORIGINS"]
            ?? configuration["CORS_ORIGINS"]
            ?? string.Empty;

        var origins = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (environment.IsDevelopment())
        {
            origins.Add("http://localhost:3000");
            origins.Add("http://127.0.0.1");
        }

        TryAddTrustedVercelOrigin(origins, context.Request.Query["redirect_uri"].FirstOrDefault());

        // Login page carries ReturnUrl=/connect/authorize?...&redirect_uri=...
        var returnUrl = context.Request.Query["ReturnUrl"].FirstOrDefault();
        if (!string.IsNullOrEmpty(returnUrl))
        {
            var decoded = Uri.UnescapeDataString(returnUrl);
            var redirectUri = ExtractQueryValue(decoded, "redirect_uri");
            TryAddTrustedVercelOrigin(origins, redirectUri);
        }

        return origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void TryAddTrustedVercelOrigin(List<string> origins, string? redirectUri)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return;

        if (IsTrustedVercelHost(uri.Host))
            origins.Add($"{uri.Scheme}://{uri.Host}");
    }

    private static bool IsTrustedVercelHost(string host) =>
        host.Equals("content-writer-v3.vercel.app", StringComparison.OrdinalIgnoreCase)
        || (host.StartsWith("content-writer-v3-", StringComparison.OrdinalIgnoreCase)
            && host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
        || host.Equals("geek-content-workflow.vercel.app", StringComparison.OrdinalIgnoreCase)
        || (host.StartsWith("geek-content-workflow-", StringComparison.OrdinalIgnoreCase)
            && host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase));

    private static string? ExtractQueryValue(string relativeOrAbsolute, string key)
    {
        var queryIndex = relativeOrAbsolute.IndexOf('?', StringComparison.Ordinal);
        var query = queryIndex >= 0 ? relativeOrAbsolute[(queryIndex + 1)..] : relativeOrAbsolute;
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=', StringComparison.Ordinal);
            if (eq <= 0) continue;
            if (!part[..eq].Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
            return Uri.UnescapeDataString(part[(eq + 1)..]);
        }

        return null;
    }
}
