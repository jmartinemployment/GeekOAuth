using GeekOAuth.Abstractions.Services;
using Serilog;

namespace GeekOAuth.Application.Services;

public sealed class AuditLogger : IAuditLogger
{
    private static readonly Serilog.ILogger Logger = Log.ForContext("SourceContext", "Audit");

    public void LoginSuccess(string userId, string ip, string? clientId) =>
        Logger.Information("LoginSuccess {@Event}", new { UserId = userId, IP = ip, ClientId = clientId, Timestamp = DateTimeOffset.UtcNow });

    public void LoginFailure(string email, string ip, string reason, int lockoutCount) =>
        Logger.Warning("LoginFailure {@Event}", new { Email = email, IP = ip, Reason = reason, LockoutCount = lockoutCount, Timestamp = DateTimeOffset.UtcNow });

    public void TwoFactorSuccess(string userId, string ip) =>
        Logger.Information("TwoFactorSuccess {@Event}", new { UserId = userId, IP = ip, Timestamp = DateTimeOffset.UtcNow });

    public void TwoFactorFailure(string userId, string ip) =>
        Logger.Warning("TwoFactorFailure {@Event}", new { UserId = userId, IP = ip, Timestamp = DateTimeOffset.UtcNow });

    public void TokenIssued(string grantType, string clientId, string? userId, IEnumerable<string> scopes) =>
        Logger.Information("TokenIssued {@Event}", new { GrantType = grantType, ClientId = clientId, UserId = userId, Scopes = scopes, Timestamp = DateTimeOffset.UtcNow });

    public void TokenRevoked(string token, string? clientId) =>
        Logger.Information("TokenRevoked {@Event}", new { Token = token[..Math.Min(8, token.Length)] + "…", ClientId = clientId, Timestamp = DateTimeOffset.UtcNow });

    public void ClientRegistered(string clientId) =>
        Logger.Information("ClientRegistered {@Event}", new { ClientId = clientId, Timestamp = DateTimeOffset.UtcNow });
}
