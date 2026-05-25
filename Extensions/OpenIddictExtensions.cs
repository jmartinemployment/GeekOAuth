using GeekOAuth.Data;
using GeekOAuth.Infrastructure;

namespace GeekOAuth.Extensions;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddGeekOpenIddict(this IServiceCollection services, IConfiguration config)
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
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetEndSessionEndpointUris("/connect/logout");

                options.AllowAuthorizationCodeFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.RequireProofKeyForCodeExchange();

                options.DisableAccessTokenEncryption();

                // RFC 8252 §7.3: loopback redirect URI port validation is skipped automatically
                // by OpenIddict when the registered URI is a loopback address (127.0.0.1 / localhost).

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15))
                    .SetIdentityTokenLifetime(TimeSpan.FromMinutes(5))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                var (signingCert, encryptionCert) = SigningCertificateLoader.Load(config);

                if (signingCert is not null)
                    options.AddSigningCertificate(signingCert);
                else
                    options.AddEphemeralSigningKey();

                if (encryptionCert is not null)
                    options.AddEncryptionCertificate(encryptionCert);
                else
                    options.AddEphemeralEncryptionKey();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
