using System.Net;
using System.Text.RegularExpressions;

namespace GeekOAuth.IntegrationTests.Helpers;

internal static partial class HtmlFormHelper
{
    public static string ExtractAntiforgeryToken(string html)
    {
        var match = AntiforgeryRegex().Match(html);
        if (!match.Success)
            throw new InvalidOperationException("Antiforgery token not found in HTML response.");

        return match.Groups[1].Value;
    }

    public static async Task<string?> FollowForAuthorizationCodeAsync(
        HttpClient client,
        string initialPath,
        int maxHops = 16)
    {
        var current = initialPath;

        for (var i = 0; i < maxHops; i++)
        {
            using var response = await client.GetAsync(current);

            if (TryExtractCodeFromRedirect(response, client, out var codeFromRedirect))
                return codeFromRedirect;

            if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.SeeOther)
            {
                current = ResolveRedirectPath(response, client);
                continue;
            }

            if (response.IsSuccessStatusCode)
            {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("Authorize application", StringComparison.Ordinal))
                {
                    current = await PostConsentGrantAsync(client, html);
                    continue;
                }
            }

            break;
        }

        return null;
    }

    private static bool TryExtractCodeFromRedirect(HttpResponseMessage response, HttpClient client, out string? code)
    {
        code = null;
        if (response.StatusCode is not (HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.SeeOther))
            return false;

        var location = response.Headers.Location;
        if (location is null)
            return false;

        var target = location.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);
        if (!target.Query.Contains("code=", StringComparison.Ordinal))
            return false;

        code = ExtractQueryParameter(target.Query, "code");
        return !string.IsNullOrEmpty(code);
    }

    private static string ResolveRedirectPath(HttpResponseMessage response, HttpClient client)
    {
        var location = response.Headers.Location
            ?? throw new InvalidOperationException("Redirect without Location header.");

        var target = location.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);
        return target.PathAndQuery;
    }

    private static async Task<string> PostConsentGrantAsync(HttpClient client, string consentHtml)
    {
        var token = ExtractAntiforgeryToken(consentHtml);
        var returnUrl = ExtractHiddenInput(consentHtml, "returnUrl")
            ?? throw new InvalidOperationException("Consent returnUrl missing.");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["consent"] = "grant",
            ["returnUrl"] = returnUrl,
            ["__RequestVerificationToken"] = token,
        });

        using var post = await client.PostAsync("/Consent", form);
        if (TryExtractCodeFromRedirect(post, client, out _))
            return returnUrl;

        return ResolveRedirectPath(post, client);
    }

    private static string? ExtractHiddenInput(string html, string name)
    {
        var pattern = $"name=\"{name}\"[^>]*value=\"([^\"]+)\"";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }

    private static string? ExtractQueryParameter(string query, string key)
    {
        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0] == key)
                return Uri.UnescapeDataString(kv[1]);
        }

        return null;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex AntiforgeryRegex();
}
