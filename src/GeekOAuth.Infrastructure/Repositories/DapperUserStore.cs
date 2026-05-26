using System.Security.Claims;
using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Infrastructure.Repositories;

public class DapperUserStore(IUserRepository repo) :
    IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>,
    IUserLockoutStore<ApplicationUser>,
    IUserTwoFactorStore<ApplicationUser>,
    IUserClaimStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>,
    IUserAuthenticatorKeyStore<ApplicationUser>,
    IUserTwoFactorRecoveryCodeStore<ApplicationUser>,
    IUserAuthenticationTokenStore<ApplicationUser>
{
    // ── IUserStore ──────────────────────────────────────────────────────────

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct)
    {
        user.Id = Guid.NewGuid().ToString();
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        await repo.CreateAsync(user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct)
    {
        await repo.UpdateAsync(user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct)
    {
        await repo.DeleteAsync(user.Id);
        return IdentityResult.Success;
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.UserName);

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) =>
        await repo.FindByIdAsync(userId);

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) =>
        await repo.FindByNameAsync(normalizedUserName);

    // ── IUserPasswordStore ───────────────────────────────────────────────────

    public async Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetPasswordHashAsync(user.Id);

    public async Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken ct)
    {
        user.PasswordHash = passwordHash;
        await repo.SetPasswordHashAsync(user.Id, passwordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.PasswordHash != null);

    // ── IUserSecurityStampStore ──────────────────────────────────────────────

    public async Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetSecurityStampAsync(user.Id);

    public async Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken ct)
    {
        user.SecurityStamp = stamp;
        await repo.SetSecurityStampAsync(user.Id, stamp);
    }

    // ── IUserEmailStore ──────────────────────────────────────────────────────

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.Email);

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken ct)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public async Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetEmailConfirmedAsync(user.Id);

    public async Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken ct)
    {
        user.EmailConfirmed = confirmed;
        await repo.SetEmailConfirmedAsync(user.Id, confirmed);
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        await repo.FindByEmailAsync(normalizedEmail);

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken ct) =>
        Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken ct)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    // ── IUserLockoutStore ────────────────────────────────────────────────────

    public async Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetLockoutEndAsync(user.Id);

    public async Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken ct)
    {
        user.LockoutEnd = lockoutEnd;
        await repo.SetLockoutEndAsync(user.Id, lockoutEnd);
    }

    public async Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.IncrementAccessFailedCountAsync(user.Id);

    public async Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.ResetAccessFailedCountAsync(user.Id);

    public async Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetAccessFailedCountAsync(user.Id);

    public async Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetLockoutEnabledAsync(user.Id);

    public async Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct)
    {
        user.LockoutEnabled = enabled;
        await repo.SetLockoutEnabledAsync(user.Id, enabled);
    }

    // ── IUserTwoFactorStore ──────────────────────────────────────────────────

    public async Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetTwoFactorEnabledAsync(user.Id);

    public async Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken ct)
    {
        user.TwoFactorEnabled = enabled;
        await repo.SetTwoFactorEnabledAsync(user.Id, enabled);
    }

    // ── IUserClaimStore ──────────────────────────────────────────────────────

    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetClaimsAsync(user.Id);

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken ct) =>
        await repo.AddClaimsAsync(user.Id, claims);

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken ct) =>
        await repo.RemoveClaimsAsync(user.Id, claims);

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken ct) =>
        await repo.ReplaceClaimAsync(user.Id, claim, newClaim);

    public Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken ct) =>
        Task.FromResult<IList<ApplicationUser>>([]);

    // ── IUserRoleStore ───────────────────────────────────────────────────────

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) =>
        await repo.AddToRoleAsync(user.Id, roleName);

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) =>
        await repo.RemoveFromRoleAsync(user.Id, roleName);

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetRolesAsync(user.Id);

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) =>
        await repo.IsInRoleAsync(user.Id, roleName);

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct) =>
        await repo.GetUsersInRoleAsync(roleName);

    // ── IUserAuthenticatorKeyStore ────────────────────────────────────────────

    public async Task SetAuthenticatorKeyAsync(ApplicationUser user, string key, CancellationToken ct) =>
        await repo.SetAuthenticatorKeyAsync(user.Id, key);

    public async Task<string?> GetAuthenticatorKeyAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.GetAuthenticatorKeyAsync(user.Id);

    // ── IUserTwoFactorRecoveryCodeStore ───────────────────────────────────────

    public async Task ReplaceCodesAsync(ApplicationUser user, IEnumerable<string> recoveryCodes, CancellationToken ct) =>
        await repo.ReplaceRecoveryCodesAsync(user.Id, recoveryCodes);

    public async Task<bool> RedeemCodeAsync(ApplicationUser user, string code, CancellationToken ct) =>
        await repo.RedeemRecoveryCodeAsync(user.Id, code);

    public async Task<int> CountCodesAsync(ApplicationUser user, CancellationToken ct) =>
        await repo.CountRecoveryCodesAsync(user.Id);

    // ── IUserAuthenticationTokenStore ────────────────────────────────────────

    public async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string? value, CancellationToken ct) =>
        await repo.SetTokenAsync(user.Id, loginProvider, name, value);

    public async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken ct) =>
        await repo.RemoveTokenAsync(user.Id, loginProvider, name);

    public async Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken ct) =>
        await repo.GetTokenAsync(user.Id, loginProvider, name);

    public void Dispose() { }
}
