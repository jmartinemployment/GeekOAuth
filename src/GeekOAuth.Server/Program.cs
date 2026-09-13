using GeekOAuth.Application.DependencyInjection;
using GeekOAuth.Infrastructure.DependencyInjection;
using GeekOAuth.Server.Data;
using GeekOAuth.Server.Extensions;
using GeekOAuth.Server.Infrastructure;
using GeekOAuth.Server.Middleware;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services));

    var connectionString = builder.Configuration["DATABASE_URL"]
        ?? throw new InvalidOperationException("DATABASE_URL is required");

    var dataSource = DatabaseConnection.CreateDataSource(connectionString);

    builder.Services.AddDbContext<OpenIddictDbContext>(options =>
    {
        options.UseNpgsql(dataSource);
        options.UseOpenIddict();
    });

    builder.Services.AddGeekOAuthInfrastructure(dataSource);
    builder.Services.AddGeekOAuthApplication();
    builder.Services.AddGeekIdentity(builder.Environment);
    builder.Services.AddGeekOpenIddict(builder.Configuration, builder.Environment);

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<OpenIddictDbContext>()
        .SetApplicationName(builder.Configuration["DATA_PROTECTION_PURPOSE"] ?? "GeekOAuth")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Fail closed: never clear KnownIPNetworks/KnownProxies unless explicit trusted hops
        // are configured. Clearing them treats every client as a trusted proxy and lets
        // X-Forwarded-For bypass rate limits. See docs/forwarded-headers-trust.md.
        GeekOAuth.Server.ForwardedHeadersConfiguration.Apply(options, builder.Configuration, builder.Environment);
    });

    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
        options.Preload = true;
    });

    builder.Services.AddGeekRateLimiting();

    var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.SetIsOriginAllowed(origin =>
                  {
                      if (corsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                          return true;
                      // Content Writer V3 production + Vercel preview deployments
                      // Preview hosts must use the hyphen separator (content-writer-v3-…).
                      // Do not allow prefix-collision hosts like content-writer-v3evil.vercel.app.
                      return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                             && uri.Scheme == Uri.UriSchemeHttps
                             && (uri.Host.Equals("content-writer-v3.vercel.app", StringComparison.OrdinalIgnoreCase)
                                 || (uri.Host.StartsWith("content-writer-v3-", StringComparison.OrdinalIgnoreCase)
                                     && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                                     && uri.Host.Length > "content-writer-v3-".Length));
                  })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    builder.Services.AddAntiforgery(options =>
    {
        var isDev = builder.Environment.IsDevelopment();
        options.Cookie.Name = isDev ? "geek-af-dev" : "__Host-geek-af";
        options.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    builder.Services.AddRazorPages();
    builder.Services.AddControllers();

    builder.Services.AddHostedService<ScopeSeeder>();
    builder.Services.AddHostedService<ClientSeeder>();

    builder.Services.AddHealthChecks()
        .AddNpgSql(DatabaseConnection.NormalizeConnectionString(connectionString), name: "postgres");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OpenIddictDbContext>();
        await MigrateDatabaseWithRetryAsync(db);
        await OidcClientBootstrap.SeedPublicClientsAsync(scope.ServiceProvider);
    }

    app.UseForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseExceptionHandler("/Error");
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();

    app.MapGet("/", (HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
            ? Results.Redirect("/Account/Login?message=signed-in")
            : Results.Redirect("/Account/Login"));

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy",
                db = report.Entries.TryGetValue("postgres", out var pg) ? pg.Status.ToString() : "unknown",
                timestamp = DateTimeOffset.UtcNow,
            });
            await context.Response.WriteAsync(result);
        },
    });

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // WebApplicationFactory stops the host during integration tests — do not flush Serilog here
    // or the static logger freezes and subsequent test host startups fail.
    throw;
}
catch (Exception ex)
{
    Log.Fatal(ex, "GeekOAuth host terminated unexpectedly");
    throw;
}

public partial class Program
{
    private static async Task MigrateDatabaseWithRetryAsync(OpenIddictDbContext db)
    {
        const int maxAttempts = 12;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientDatabaseError(ex))
            {
                var delay = TimeSpan.FromSeconds(Math.Min(attempt * 3, 30));
                Log.Warning(ex, "Database not ready (attempt {Attempt}/{Max}); retrying in {Delay}s",
                    attempt, maxAttempts, delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    private static bool IsTransientDatabaseError(Exception ex) =>
        ex is NpgsqlException or TimeoutException
        || (ex.InnerException is not null && IsTransientDatabaseError(ex.InnerException));
}
