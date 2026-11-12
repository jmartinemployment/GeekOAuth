using OpenIddict.Abstractions;
using Serilog;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace GeekOAuth.Server.Infrastructure;

/// <summary>
/// Public PKCE clients required for Geek SEO web and Electron — seeded at startup before listening.
/// </summary>
internal static class OidcPublicClientSeeds
{
    /// <summary>
    /// Content Creator's clients, projects and their billing data.
    /// </summary>
    /// <remarks>
    /// Granted to geek-content-creator-v2 only. GeekAPI requires it on every project route and
    /// answers 403 without it, so this authority being shared across Geek apps does not make one
    /// app's token good for another app's client records.
    /// </remarks>
    private const string ContentCreatorManageScope = "content-creator.manage";

    /// <summary>
    /// GeekSocial's connected Pages, its reply inbox, posting and audit. Granted to geek-social only;
    /// GeekAPI requires it on every api/social route and on /hubs/social-realtime and answers 403 without it.
    /// </summary>
    private const string GeekSocialManageScope = "geeksocial.manage";

    /// <summary>
    /// Geek-CRM's leads, deals, catalog and customers. Granted to geekcrm only; GeekAPI requires it on a signed-in
    /// caller of api/crm and then reads the caller's tenant and role from tenant_users.
    /// </summary>
    private const string GeekCrmManageScope = "geekcrm.manage";

    public static async Task SeedPublicClientsAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        await SeedElectronClient(appManager, ct);
        await SeedGeekSeoWebClient(appManager, ct);
        await SeedGeekAdminClient(appManager, ct);
        await SeedGeekContentWorkflowClient(appManager, ct);
        await SeedContentWriterV3Client(appManager, ct);
        await SeedGeekContentCreatorClient(appManager, ct);
        await SeedGeekContentCreatorV2Client(appManager, ct);
        await SeedGeekCrawlerClient(appManager, ct);
        await SeedGeekImageGeneratorClient(appManager, ct);
        await SeedGeekReportsClient(appManager, ct);
        await SeedGeekSocialClient(appManager, ct);
        await SeedGeekCrmClient(appManager, ct);
    }

    private static async Task SeedGeekCrmClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geekcrm";
        Uri[] redirectUris =
        [
            new("https://geek-crm.geekatyourspot.com/auth/callback"),
            new("https://geek-crm.vercel.app/auth/callback"),
            new("http://localhost:3007/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geek-crm.geekatyourspot.com/signed-out"),
            new("https://geek-crm.vercel.app/signed-out"),
            new("http://localhost:3007/signed-out"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            // Redirect addresses and the scope are reconciled on every start, so a change here reaches
            // an environment where the client already exists, not only a fresh one.
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            var crmScope = Permissions.Prefixes.Scope + GeekCrmManageScope;
            if (!descriptor.Permissions.Contains(crmScope))
            {
                descriptor.Permissions.Add(crmScope);
                Log.Information("OpenIddict client {ClientId} granted scope {Scope}", clientId, GeekCrmManageScope);
            }
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs and permissions updated", clientId);
            return;
        }

        var create = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek-CRM",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + GeekCrmManageScope,
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        };
        foreach (var uri in redirectUris)
            create.RedirectUris.Add(uri);
        foreach (var uri in postLogoutRedirectUris)
            create.PostLogoutRedirectUris.Add(uri);
        await appManager.CreateAsync(create, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekSocialClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-social";
        Uri[] redirectUris =
        [
            new("https://geek-social.geekatyourspot.com/auth/callback"),
            new("https://geek-social.vercel.app/auth/callback"),
            new("http://localhost:3006/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geek-social.geekatyourspot.com/signed-out"),
            new("https://geek-social.vercel.app/signed-out"),
            new("http://localhost:3006/signed-out"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            // Redirect addresses and the scope are reconciled on every start, so a change here reaches
            // an environment where the client already exists, not only a fresh one.
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            var socialScope = Permissions.Prefixes.Scope + GeekSocialManageScope;
            if (!descriptor.Permissions.Contains(socialScope))
            {
                descriptor.Permissions.Add(socialScope);
                Log.Information("OpenIddict client {ClientId} granted scope {Scope}", clientId, GeekSocialManageScope);
            }
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs and permissions updated", clientId);
            return;
        }

        var create = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "GeekSocial",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + GeekSocialManageScope,
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        };
        foreach (var uri in redirectUris)
            create.RedirectUris.Add(uri);
        foreach (var uri in postLogoutRedirectUris)
            create.PostLogoutRedirectUris.Add(uri);
        await appManager.CreateAsync(create, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekReportsClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-reports";
        Uri[] redirectUris =
        [
            new("https://reports.geekatyourspot.com/auth/callback"),
            new("https://geek-reports.vercel.app/auth/callback"),
            new("http://localhost:3005/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://reports.geekatyourspot.com/"),
            new("https://geek-reports.vercel.app/"),
            new("http://localhost:3005/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        var create = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Reports",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        };
        foreach (var uri in redirectUris)
            create.RedirectUris.Add(uri);
        foreach (var uri in postLogoutRedirectUris)
            create.PostLogoutRedirectUris.Add(uri);
        await appManager.CreateAsync(create, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedElectronClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-seo-electron";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek SEO Electron",
            RedirectUris = { new Uri("http://127.0.0.1") },
            PostLogoutRedirectUris = { new Uri("http://127.0.0.1") },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + "devices.manage",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekSeoWebClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geekseo";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek SEO (Web)",
            RedirectUris =
            {
                new Uri("https://seo.geekatyourspot.com/auth/callback"),
                new Uri("http://localhost:3000/auth/callback"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://seo.geekatyourspot.com/"),
                new Uri("http://localhost:3000/"),
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekAdminClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-admin";
        if (await appManager.FindByClientIdAsync(clientId, ct) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Blog Admin",
            RedirectUris =
            {
                new Uri("https://admin.geekatyourspot.com/auth/callback"),
                new Uri("http://localhost:3002/auth/callback"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://admin.geekatyourspot.com/"),
                new Uri("http://localhost:3002/"),
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekContentWorkflowClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-content-workflow";
        Uri[] redirectUris =
        [
            new("https://geekcontentworkflow.geekatyourspot.com/auth/callback"),
            new("https://geek-content-workflow.vercel.app/auth/callback"),
            new("http://localhost:3000/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geekcontentworkflow.geekatyourspot.com/"),
            new("https://geek-content-workflow.vercel.app/"),
            new("http://localhost:3000/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Content Workflow",
            RedirectUris =
            {
                redirectUris[0], redirectUris[1], redirectUris[2],
            },
            PostLogoutRedirectUris =
            {
                postLogoutRedirectUris[0], postLogoutRedirectUris[1], postLogoutRedirectUris[2],
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekContentCreatorClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-content-creator";
        Uri[] redirectUris =
        [
            new("https://geek-content-creator.geekatyourspot.com/auth/callback"),
            new("https://geek-content-creator.vercel.app/auth/callback"),
            new("http://localhost:3003/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geek-content-creator.geekatyourspot.com/"),
            new("https://geek-content-creator.vercel.app/"),
            new("http://localhost:3003/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Content Creator",
            RedirectUris =
            {
                redirectUris[0], redirectUris[1], redirectUris[2],
            },
            PostLogoutRedirectUris =
            {
                postLogoutRedirectUris[0], postLogoutRedirectUris[1], postLogoutRedirectUris[2],
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekContentCreatorV2Client(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-content-creator-v2";
        // OpenIddict matches redirect_uri exactly — no prefix patterns, unlike the CSP host
        // allowlist in SecurityHeadersMiddleware. Every host the app is actually served from has to
        // be listed here or sign-in fails with ID2043.
        //
        // The project-alias hosts below are what Vercel serves this project on today. They are
        // stable aliases, not per-deployment URLs (those carry a build hash and change every push).
        Uri[] redirectUris =
        [
            new("https://content-creator-v2-phi.vercel.app/auth/callback"),
            new("https://content-creator-v2.vercel.app/auth/callback"),
            new("https://content-creator-v2-jeff-martins-projects-66716453.vercel.app/auth/callback"),
            new("https://content-creator-v2-git-main-jeff-martins-projects-66716453.vercel.app/auth/callback"),
            new("http://localhost:3004/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://content-creator-v2-phi.vercel.app/"),
            new("https://content-creator-v2.vercel.app/"),
            new("https://content-creator-v2-jeff-martins-projects-66716453.vercel.app/"),
            new("https://content-creator-v2-git-main-jeff-martins-projects-66716453.vercel.app/"),
            new("http://localhost:3004/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            // Permissions are reconciled here too, not only on create. This client already exists
            // in every environment that matters, so a scope added to the create path alone would
            // never reach it: the app would request content-creator.manage, OpenIddict would
            // refuse to issue it, and GeekAPI would answer 403 to a user who looks correctly
            // signed in. Same class of bug as the one noted below about redirect URIs, on the
            // other field.
            var contentCreatorScope = Permissions.Prefixes.Scope + ContentCreatorManageScope;
            if (!descriptor.Permissions.Contains(contentCreatorScope))
            {
                descriptor.Permissions.Add(contentCreatorScope);
                Log.Information(
                    "OpenIddict client {ClientId} granted scope {Scope}",
                    clientId,
                    ContentCreatorManageScope);
            }
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs and permissions updated", clientId);
            return;
        }

        // Built from the arrays rather than indexed into them: the previous form listed
        // redirectUris[0..2] explicitly, so adding a host here would have been silently dropped on a
        // fresh create while working on update — the two paths disagreeing about the same client.
        var descriptorToCreate = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Content Creator v2",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
                Permissions.Prefixes.Scope + ContentCreatorManageScope,
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        };
        foreach (var uri in redirectUris)
            descriptorToCreate.RedirectUris.Add(uri);
        foreach (var uri in postLogoutRedirectUris)
            descriptorToCreate.PostLogoutRedirectUris.Add(uri);

        await appManager.CreateAsync(descriptorToCreate, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekCrawlerClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-crawler";
        Uri[] redirectUris =
        [
            new("https://geek-crawler.vercel.app/auth/callback"),
            new("http://localhost:3000/auth/callback"),
            new("http://localhost:3001/auth/callback"),
            new("http://localhost:3005/auth/callback"),
            new("http://127.0.0.1:3000/auth/callback"),
            new("http://127.0.0.1:3001/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geek-crawler.vercel.app/"),
            new("http://localhost:3000/"),
            new("http://localhost:3001/"),
            new("http://localhost:3005/"),
            new("http://127.0.0.1:3000/"),
            new("http://127.0.0.1:3001/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        var create = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek-Crawler",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        };
        foreach (var uri in redirectUris)
            create.RedirectUris.Add(uri);
        foreach (var uri in postLogoutRedirectUris)
            create.PostLogoutRedirectUris.Add(uri);
        await appManager.CreateAsync(create, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedGeekImageGeneratorClient(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "geek-image-generator";
        Uri[] redirectUris =
        [
            new("https://geek-image-generator.geekatyourspot.com/auth/callback"),
            new("https://image-generator.geekatyourspot.com/auth/callback"),
            new("http://localhost:3000/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://geek-image-generator.geekatyourspot.com/"),
            new("https://image-generator.geekatyourspot.com/"),
            new("http://localhost:3000/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Geek Image Generator",
            RedirectUris =
            {
                redirectUris[0], redirectUris[1], redirectUris[2],
            },
            PostLogoutRedirectUris =
            {
                postLogoutRedirectUris[0], postLogoutRedirectUris[1], postLogoutRedirectUris[2],
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }

    private static async Task SeedContentWriterV3Client(IOpenIddictApplicationManager appManager, CancellationToken ct)
    {
        const string clientId = "content-writer-v3";
        Uri[] redirectUris =
        [
            new("https://content-writer.geekatyourspot.com/auth/callback"),
            new("https://content-writer-v3.vercel.app/auth/callback"),
            new("https://content-writer-v3-git-main-jeff-martins-projects-66716453.vercel.app/auth/callback"),
            new("http://localhost:3000/auth/callback"),
        ];
        Uri[] postLogoutRedirectUris =
        [
            new("https://content-writer.geekatyourspot.com/"),
            new("https://content-writer-v3.vercel.app/"),
            new("https://content-writer-v3-git-main-jeff-martins-projects-66716453.vercel.app/"),
            new("http://localhost:3000/"),
        ];

        var existing = await appManager.FindByClientIdAsync(clientId, ct);
        if (existing is not null)
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(descriptor, existing, ct);
            descriptor.RedirectUris.Clear();
            foreach (var uri in redirectUris)
                descriptor.RedirectUris.Add(uri);
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in postLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(uri);
            await appManager.UpdateAsync(existing, descriptor, ct);
            Log.Information("OpenIddict client {ClientId} redirect URIs updated", clientId);
            return;
        }

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Content Writer V3",
            RedirectUris =
            {
                redirectUris[0], redirectUris[1], redirectUris[2], redirectUris[3],
            },
            PostLogoutRedirectUris =
            {
                postLogoutRedirectUris[0], postLogoutRedirectUris[1], postLogoutRedirectUris[2], postLogoutRedirectUris[3],
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "openid",
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            ConsentType = ConsentTypes.Implicit,
        }, ct);
        Log.Information("OpenIddict client {ClientId} created", clientId);
    }
}
