namespace GeekOAuth.Abstractions.Services;

public interface IUserAccountService
{
    Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}

/// <param name="AutoConfirmed">True when registration confirmed the email itself because no SMTP server is configured, so no confirmation email was sent.</param>
public sealed record AccountOperationResult(bool Succeeded, string? Error = null, string? UserId = null, bool AutoConfirmed = false);
