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
        headers["Content-Security-Policy"] = BuildContentSecurityPolicy();
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        await next(context);
    }

    private string BuildContentSecurityPolicy()
    {
        // Chrome validates the entire redirect chain after a form POST. Login posts to
        // /Account/Login, then redirects through /connect/authorize to the client's
        // redirect_uri (e.g. seo.geekatyourspot.com). Those cross-origin hops must be
        // listed in form-action or Chrome blocks sign-in with a misleading console error.
        var formActionOrigins = ResolveFormActionOrigins();
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

    private string[] ResolveFormActionOrigins()
    {
        var raw = configuration["FORM_ACTION_ORIGINS"]
            ?? configuration["CORS_ORIGINS"]
            ?? string.Empty;

        var origins = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (environment.IsDevelopment())
        {
            origins =
            [
                ..origins,
                "http://localhost:3000",
                "http://127.0.0.1",
            ];
        }

        return origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
