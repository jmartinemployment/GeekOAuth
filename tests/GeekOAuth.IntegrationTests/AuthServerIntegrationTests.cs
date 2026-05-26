using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GeekOAuth.IntegrationTests;

[Collection(AuthServerCollection.Name)]
public sealed class AuthServerIntegrationTests(GeekOAuthWebApplicationFactory factory)
{
    private HttpClient Client => field ??= factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    [DockerFact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await Client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
    }

    [DockerFact]
    public async Task OpenIdConfiguration_IncludesAuthorizeEndpoint()
    {
        var response = await Client.GetAsync("/.well-known/openid-configuration");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var authorize = doc.RootElement.GetProperty("authorization_endpoint").GetString();
        Assert.Contains("/connect/authorize", authorize, StringComparison.Ordinal);
    }

    [DockerFact]
    public async Task RegisterPage_ReturnsOk()
    {
        var response = await Client.GetAsync("/Account/Register");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task LoginPage_ReturnsOk()
    {
        var response = await Client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [DockerFact]
    public async Task TokenEndpoint_WithoutCredentials_ReturnsBadRequest()
    {
        var response = await Client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
        }));

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized,
            $"Expected 400/401, got {(int)response.StatusCode}");
    }
}
