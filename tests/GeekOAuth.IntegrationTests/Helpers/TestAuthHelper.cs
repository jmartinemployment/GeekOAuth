using GeekOAuth.Abstractions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GeekOAuth.IntegrationTests.Helpers;

internal static class TestAuthHelper
{
    public const string DefaultPassword = "IntegrationTest!1AaBb";

    public static async Task<string> SeedConfirmedUserAsync(IServiceProvider services, string? email = null)
    {
        email ??= $"user-{Guid.NewGuid():N}@integration.test";

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return email;
    }

    public static async Task LoginViaFormAsync(HttpClient client, string email, string password)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        loginPage.EnsureSuccessStatusCode();
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = HtmlFormHelper.ExtractAntiforgeryToken(html);

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        });

        using var response = await client.PostAsync("/Account/Login", form);
        if (response.Headers.Location is null && !response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Login failed: {(int)response.StatusCode}");
    }
}
