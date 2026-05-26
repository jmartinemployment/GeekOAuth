namespace GeekOAuth.Server.Infrastructure;

/// <summary>
/// Runs work after Kestrel is listening — avoids OpenIddict/DB startup deadlocks.
/// </summary>
public abstract class ApplicationStartedHostedService(IHostApplicationLifetime lifetime) : IHostedService
{
    private static readonly SemaphoreSlim StartupGate = new(1, 1);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            _ = RunWhenStartedAsync();
        });

        return Task.CompletedTask;
    }

    private async Task RunWhenStartedAsync()
    {
        await StartupGate.WaitAsync();
        try
        {
            await ExecuteAsync(CancellationToken.None);
        }
        finally
        {
            StartupGate.Release();
        }
    }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
