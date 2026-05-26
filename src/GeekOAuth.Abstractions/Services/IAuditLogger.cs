namespace GeekOAuth.Abstractions.Services;

public interface IAuditLogger
{
    void LoginSuccess(string userId, string ip, string? clientId);
    void LoginFailure(string email, string ip, string reason, int lockoutCount);
    void TwoFactorSuccess(string userId, string ip);
    void TwoFactorFailure(string userId, string ip);
    void TokenIssued(string grantType, string clientId, string? userId, IEnumerable<string> scopes);
    void TokenRevoked(string token, string? clientId);
    void ClientRegistered(string clientId);
}
