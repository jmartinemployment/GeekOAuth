namespace GeekOAuth.IntegrationTests.Helpers;

internal static class ServerReadiness
{
    public static async Task WaitUntilReadyAsync(HttpClient client, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(60);
        var deadline = DateTimeOffset.UtcNow.Add(timeout.Value);

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var health = await client.GetAsync("/health");
                var discovery = await client.GetAsync("/.well-known/openid-configuration");
                if (health.IsSuccessStatusCode && discovery.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
                // Host still starting.
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("GeekOAuth test host did not become ready in time.");
    }
}
