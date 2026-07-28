using System.Text.Json;

namespace GeekOAuth.Server.Infrastructure;

/// <summary>Temporary debug-session NDJSON logger (session 2d6b04). Remove after verification.</summary>
internal static class AgentDebugLog
{
    private const string SessionId = "2d6b04";
    private const string LogPath = "/Users/jeffmartin/development/GeekOAuth/.cursor/debug-2d6b04.log";

    public static void Write(string hypothesisId, string location, string message, object? data = null)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["sessionId"] = SessionId,
                ["hypothesisId"] = hypothesisId,
                ["location"] = location,
                ["message"] = message,
                ["data"] = data,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            });
            File.AppendAllText(LogPath, payload + Environment.NewLine);
        }
        catch
        {
            // Production containers cannot write the local debug path — Serilog still records seed events.
        }
    }
}
