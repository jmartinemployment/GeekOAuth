namespace GeekOAuth.Abstractions.Services;

public interface IUserAccountService
{
    Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the login of the one owner of a Geek-Receptionist tenant, with the tenant carried on the user so every
    /// token issued to that owner has a tenant_id claim. Called only by GeekAPI after the tenant's subscription is
    /// active. A tenant that already has an owner, or an email that already has a login, is a <see cref="AccountOperationResult.Conflict"/>.
    /// </summary>
    Task<AccountOperationResult> ProvisionTenantOwnerAsync(Guid tenantId, string email, string password, string? displayName, CancellationToken cancellationToken = default);

    Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<AccountOperationResult> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}

public sealed record AccountOperationResult(bool Succeeded, string? Error = null, string? UserId = null, bool Conflict = false);
