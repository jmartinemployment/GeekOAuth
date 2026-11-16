namespace GeekOAuth.Abstractions.Services;

public interface IUserAccountService
{
    /// <param name="returnUrl">The authorize request the person came from, when they started signing up from an app. Carried in the confirmation link so they can finish signing in to that app; ignored unless <see cref="GeekOAuth.Abstractions.Security.AuthorizeReturnUrl"/> accepts it.</param>
    Task<AccountOperationResult> RegisterAsync(string email, string password, string? returnUrl = null, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}

/// <param name="AutoConfirmed">True when registration confirmed the email itself because no SMTP server is configured, so no confirmation email was sent.</param>
public sealed record AccountOperationResult(bool Succeeded, string? Error = null, string? UserId = null, bool AutoConfirmed = false);
