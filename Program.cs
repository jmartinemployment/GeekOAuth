using System.Data;
using GeekOAuth.Data;
using GeekOAuth.Extensions;
using GeekOAuth.Infrastructure;
using GeekOAuth.Middleware;
using GeekOAuth.Models;
using GeekOAuth.Repositories;
using GeekOAuth.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
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

    // ── Database ─────────────────────────────────────────────────────────────

    var connectionString = builder.Configuration["DATABASE_URL"]
        ?? throw new InvalidOperationException("DATABASE_URL is required");

    builder.Services.AddDbContext<OpenIddictDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.UseOpenIddict();
    });

    // ── Dapper connection factory ─────────────────────────────────────────────

    builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
        new NpgsqlConnectionFactory(connectionString));

    // ── Identity (Dapper-backed) ──────────────────────────────────────────────

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserStore<ApplicationUser>, DapperUserStore>();
    builder.Services.AddScoped<IRoleStore<IdentityRole>, NoOpRoleStore>();
    builder.Services.AddGeekIdentity();

    // ── OpenIddict ────────────────────────────────────────────────────────────

    builder.Services.AddGeekOpenIddict(builder.Configuration);

    // ── Data Protection ───────────────────────────────────────────────────────

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<OpenIddictDbContext>()
        .SetApplicationName(
            builder.Configuration["DATA_PROTECTION_PURPOSE"] ?? "GeekOAuth")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    // ── Rate limiting ─────────────────────────────────────────────────────────

    builder.Services.AddGeekRateLimiting();

    // ── CORS ──────────────────────────────────────────────────────────────────

    var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // ── Razor Pages + MVC controllers ────────────────────────────────────────

    builder.Services.AddRazorPages();
    builder.Services.AddControllers();

    // ── Seeders ───────────────────────────────────────────────────────────────

    builder.Services.AddHostedService<ScopeSeeder>();
    builder.Services.AddHostedService<ClientSeeder>();

    // ── Misc services ─────────────────────────────────────────────────────────

    builder.Services.AddSingleton<AuditLogger>();

    // ── Health checks ─────────────────────────────────────────────────────────

    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres");

    // ── App pipeline ──────────────────────────────────────────────────────────

    var app = builder.Build();

    // Run EF migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OpenIddictDbContext>();
        await db.Database.MigrateAsync();
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

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy",
                db = report.Entries.TryGetValue("postgres", out var pg)
                    ? pg.Status.ToString()
                    : "unknown",
                timestamp = DateTimeOffset.UtcNow,
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Rate limit policies on specific endpoints
    app.MapPost("/connect/token", () => Results.Empty)
        .RequireRateLimiting("token-endpoint")
        .WithOrder(-1);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "GeekOAuth host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
