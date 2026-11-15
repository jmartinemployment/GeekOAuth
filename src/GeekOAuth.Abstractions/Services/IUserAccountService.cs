namespace GeekOAuth.Abstractions.Services;

public interface IUserAccountService
{
    Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}

public sealed record AccountOperationResult(bool Succeeded, string? Error = null, string? UserId = null);
