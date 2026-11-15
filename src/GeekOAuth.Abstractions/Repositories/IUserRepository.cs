using System.Security.Claims;
using GeekOAuth.Abstractions.Models;

namespace GeekOAuth.Abstractions.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<ApplicationUser?> FindByNameAsync(string normalizedUserName);
    Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail);
    Task CreateAsync(ApplicationUser user);
    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(string userId);

    Task<string?> GetPasswordHashAsync(string userId);
    Task SetPasswordHashAsync(string userId, string? passwordHash);

    Task<string?> GetSecurityStampAsync(string userId);
    Task SetSecurityStampAsync(string userId, string stamp);

    Task<bool> GetEmailConfirmedAsync(string userId);
    Task SetEmailConfirmedAsync(string userId, bool confirmed);

    Task<DateTimeOffset?> GetLockoutEndAsync(string userId);
    Task SetLockoutEndAsync(string userId, DateTimeOffset? lockoutEnd);
    Task<int> GetAccessFailedCountAsync(string userId);
    Task<int> IncrementAccessFailedCountAsync(string userId);
    Task ResetAccessFailedCountAsync(string userId);
    Task<bool> GetLockoutEnabledAsync(string userId);
    Task SetLockoutEnabledAsync(string userId, bool enabled);

    Task<bool> GetTwoFactorEnabledAsync(string userId);
    Task SetTwoFactorEnabledAsync(string userId, bool enabled);

    Task<string?> GetAuthenticatorKeyAsync(string userId);
    Task SetAuthenticatorKeyAsync(string userId, string? key);

    Task<IList<string>> GetRolesAsync(string userId);
    Task AddToRoleAsync(string userId, string normalizedRoleName);
    Task RemoveFromRoleAsync(string userId, string normalizedRoleName);
    Task<bool> IsInRoleAsync(string userId, string normalizedRoleName);
    Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRoleName);

    Task<IList<Claim>> GetClaimsAsync(string userId);
    Task AddClaimsAsync(string userId, IEnumerable<Claim> claims);
    Task RemoveClaimsAsync(string userId, IEnumerable<Claim> claims);
    Task ReplaceClaimAsync(string userId, Claim claim, Claim newClaim);

    Task<IEnumerable<string>> GetRecoveryCodesAsync(string userId);
    Task ReplaceRecoveryCodesAsync(string userId, IEnumerable<string> codes);
    Task<bool> RedeemRecoveryCodeAsync(string userId, string code);
    Task<int> CountRecoveryCodesAsync(string userId);

    Task<string?> GetTokenAsync(string userId, string loginProvider, string name);
    Task SetTokenAsync(string userId, string loginProvider, string name, string? value);
    Task RemoveTokenAsync(string userId, string loginProvider, string name);
}
