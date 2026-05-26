using Docker.DotNet;

namespace GeekOAuth.IntegrationTests;

/// <summary>Skips the test when Docker is not running (required for Testcontainers).</summary>
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!IsDockerAvailable())
            Skip = "Docker is not available — start Docker Desktop to run integration tests.";
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            using var client = new DockerClientConfiguration().CreateClient();
            client.System.PingAsync().GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
