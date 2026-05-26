using System.Net;
using System.Text.Json;
using GeekOAuth.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GeekOAuth.IntegrationTests;

[Collection(AuthServerCollection.Name)]
public sealed class PkceFlowTests(GeekOAuthWebApplicationFactory factory)
{
    [DockerFact]
    public async Task AuthorizationCode_WithPkce_ReturnsJwtAccessToken()
    {
        using var warmup = factory.CreateClient();
        await ServerReadiness.WaitUntilReadyAsync(warmup);

        var email = await TestAuthHelper.SeedConfirmedUserAsync(factory.Services);

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri($"{GeekOAuthWebApplicationFactory.TestBaseUrl}/"),
        };

        await TestAuthHelper.LoginViaFormAsync(client, email, TestAuthHelper.DefaultPassword);

        var (verifier, challenge) = PkceHelper.Generate();
        var state = Guid.NewGuid().ToString("N");
        const string clientId = "geek-seo-electron";
        const string redirectUri = "http://127.0.0.1/";

        var authorizePath =
            "/connect/authorize" +
            $"?client_id={clientId}" +
            "&response_type=code" +
            "&scope=openid%20profile%20email" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&code_challenge={challenge}" +
            "&code_challenge_method=S256" +
            $"&state={state}";

        var code = await HtmlFormHelper.FollowForAuthorizationCodeAsync(client, authorizePath);
        Assert.False(string.IsNullOrEmpty(code), "Authorization code was not returned.");

        var tokenForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = clientId,
            ["code"] = code!,
            ["redirect_uri"] = redirectUri,
            ["code_verifier"] = verifier,
        });

        using var tokenResponse = await client.PostAsync("/connect/token", tokenForm);
        var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
        Assert.True(tokenResponse.IsSuccessStatusCode, tokenBody);

        using var doc = JsonDocument.Parse(tokenBody);
        Assert.True(doc.RootElement.TryGetProperty("access_token", out var accessToken));
        Assert.False(string.IsNullOrEmpty(accessToken.GetString()));
        Assert.Equal("Bearer", doc.RootElement.GetProperty("token_type").GetString());
    }

    [DockerFact]
    public async Task ClientCredentials_GeekApi_ReturnsAccessToken()
    {
        using var client = factory.CreateClient();

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "geekapi",
            ["client_secret"] = "integration-test-geekapi-secret-32chars!",
            ["scope"] = "internal.api",
        });

        using var response = await client.PostAsync("/connect/token", form);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("access_token", out _));
    }
}
