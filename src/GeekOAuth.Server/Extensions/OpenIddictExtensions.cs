using GeekOAuth.Server.Data;
using GeekOAuth.Server.Infrastructure;

namespace GeekOAuth.Server.Extensions;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddGeekOpenIddict(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment environment)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<OpenIddictDbContext>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(config["ISSUER_URL"]
                    ?? throw new InvalidOperationException("ISSUER_URL is required")));

                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetRevocationEndpointUris("/connect/revoke")
                    .SetEndSessionEndpointUris("/connect/logout");

                options.AllowAuthorizationCodeFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.RequireProofKeyForCodeExchange();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15))
                    .SetIdentityTokenLifetime(TimeSpan.FromMinutes(5))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                options.DisableAccessTokenEncryption();

                var (signingCert, encryptionCert) = SigningCertificateLoader.Load(config);

                if (signingCert is not null)
                    options.AddSigningCertificate(signingCert);
                else if (environment.IsDevelopment())
                    options.AddEphemeralSigningKey();
                else
                    throw new InvalidOperationException("SIGNING_CERT_BASE64 is required outside Development.");

                if (encryptionCert is not null)
                    options.AddEncryptionCertificate(encryptionCert);
                else if (signingCert is not null)
                    options.AddEncryptionCertificate(signingCert);
                else if (environment.IsDevelopment())
                    options.AddEphemeralEncryptionKey();
                else
                    throw new InvalidOperationException(
                        "ENCRYPTION_CERT_BASE64 or SIGNING_CERT_BASE64 is required outside Development.");

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                // Railway / reverse-proxy: TLS terminates at edge; Kestrel sees HTTP.
                if (!environment.IsDevelopment())
                    aspNetCore.DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
