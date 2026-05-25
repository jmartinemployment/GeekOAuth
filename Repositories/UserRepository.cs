using System.Data;
using System.Security.Claims;
using Dapper;
using GeekOAuth.Models;
using Npgsql;

namespace GeekOAuth.Repositories;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    private IDbConnection Connect() => connectionFactory.CreateConnection();

    public async Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<ApplicationUser>(
            "SELECT * FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<ApplicationUser>(
            "SELECT * FROM asp_net_users WHERE normalized_user_name = @Name", new { Name = normalizedUserName });
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<ApplicationUser>(
            "SELECT * FROM asp_net_users WHERE normalized_email = @Email", new { Email = normalizedEmail });
    }

    public async Task CreateAsync(ApplicationUser user)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            INSERT INTO asp_net_users (
                id, user_name, normalized_user_name, email, normalized_email,
                email_confirmed, password_hash, security_stamp, concurrency_stamp,
                phone_number, phone_number_confirmed, two_factor_enabled,
                lockout_end, lockout_enabled, access_failed_count,
                display_name, tenant
            ) VALUES (
                @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail,
                @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
                @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled,
                @LockoutEnd, @LockoutEnabled, @AccessFailedCount,
                @DisplayName, @Tenant
            )", user);
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            UPDATE asp_net_users SET
                user_name = @UserName,
                normalized_user_name = @NormalizedUserName,
                email = @Email,
                normalized_email = @NormalizedEmail,
                email_confirmed = @EmailConfirmed,
                password_hash = @PasswordHash,
                security_stamp = @SecurityStamp,
                concurrency_stamp = @ConcurrencyStamp,
                phone_number = @PhoneNumber,
                phone_number_confirmed = @PhoneNumberConfirmed,
                two_factor_enabled = @TwoFactorEnabled,
                lockout_end = @LockoutEnd,
                lockout_enabled = @LockoutEnabled,
                access_failed_count = @AccessFailedCount,
                display_name = @DisplayName,
                tenant = @Tenant
            WHERE id = @Id", user);
    }

    public async Task DeleteAsync(string userId)
    {
        using var db = Connect();
        await db.ExecuteAsync("DELETE FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task<string?> GetPasswordHashAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<string?>(
            "SELECT password_hash FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetPasswordHashAsync(string userId, string? passwordHash)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET password_hash = @Hash WHERE id = @UserId",
            new { Hash = passwordHash, UserId = userId });
    }

    public async Task<string?> GetSecurityStampAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<string?>(
            "SELECT security_stamp FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetSecurityStampAsync(string userId, string stamp)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET security_stamp = @Stamp WHERE id = @UserId",
            new { Stamp = stamp, UserId = userId });
    }

    public async Task<bool> GetEmailConfirmedAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT email_confirmed FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetEmailConfirmedAsync(string userId, bool confirmed)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET email_confirmed = @Confirmed WHERE id = @UserId",
            new { Confirmed = confirmed, UserId = userId });
    }

    public async Task<DateTimeOffset?> GetLockoutEndAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<DateTimeOffset?>(
            "SELECT lockout_end FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetLockoutEndAsync(string userId, DateTimeOffset? lockoutEnd)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET lockout_end = @LockoutEnd WHERE id = @UserId",
            new { LockoutEnd = lockoutEnd, UserId = userId });
    }

    public async Task<int> GetAccessFailedCountAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<int>(
            "SELECT access_failed_count FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task<int> IncrementAccessFailedCountAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleAsync<int>(@"
            UPDATE asp_net_users
            SET access_failed_count = access_failed_count + 1
            WHERE id = @UserId
            RETURNING access_failed_count", new { UserId = userId });
    }

    public async Task ResetAccessFailedCountAsync(string userId)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET access_failed_count = 0 WHERE id = @UserId",
            new { UserId = userId });
    }

    public async Task<bool> GetLockoutEnabledAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT lockout_enabled FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetLockoutEnabledAsync(string userId, bool enabled)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET lockout_enabled = @Enabled WHERE id = @UserId",
            new { Enabled = enabled, UserId = userId });
    }

    public async Task<bool> GetTwoFactorEnabledAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<bool>(
            "SELECT two_factor_enabled FROM asp_net_users WHERE id = @UserId", new { UserId = userId });
    }

    public async Task SetTwoFactorEnabledAsync(string userId, bool enabled)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "UPDATE asp_net_users SET two_factor_enabled = @Enabled WHERE id = @UserId",
            new { Enabled = enabled, UserId = userId });
    }

    public async Task<string?> GetAuthenticatorKeyAsync(string userId)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<string?>(
            "SELECT value FROM asp_net_user_tokens WHERE user_id = @UserId AND login_provider = '[AspNetUserStore]' AND name = 'AuthenticatorKey'",
            new { UserId = userId });
    }

    public async Task SetAuthenticatorKeyAsync(string userId, string? key)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            INSERT INTO asp_net_user_tokens (user_id, login_provider, name, value)
            VALUES (@UserId, '[AspNetUserStore]', 'AuthenticatorKey', @Key)
            ON CONFLICT (user_id, login_provider, name)
            DO UPDATE SET value = @Key",
            new { UserId = userId, Key = key });
    }

    public async Task<IList<string>> GetRolesAsync(string userId)
    {
        using var db = Connect();
        var roles = await db.QueryAsync<string>(@"
            SELECT r.name FROM asp_net_roles r
            INNER JOIN asp_net_user_roles ur ON r.id = ur.role_id
            WHERE ur.user_id = @UserId", new { UserId = userId });
        return roles.ToList();
    }

    public async Task AddToRoleAsync(string userId, string normalizedRoleName)
    {
        using var db = Connect();
        var roleId = await db.QuerySingleOrDefaultAsync<string>(
            "SELECT id FROM asp_net_roles WHERE normalized_name = @Name", new { Name = normalizedRoleName });
        if (roleId is null) return;
        await db.ExecuteAsync(
            "INSERT INTO asp_net_user_roles (user_id, role_id) VALUES (@UserId, @RoleId) ON CONFLICT DO NOTHING",
            new { UserId = userId, RoleId = roleId });
    }

    public async Task RemoveFromRoleAsync(string userId, string normalizedRoleName)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            DELETE FROM asp_net_user_roles
            WHERE user_id = @UserId AND role_id = (
                SELECT id FROM asp_net_roles WHERE normalized_name = @Name
            )", new { UserId = userId, Name = normalizedRoleName });
    }

    public async Task<bool> IsInRoleAsync(string userId, string normalizedRoleName)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<bool>(@"
            SELECT EXISTS (
                SELECT 1 FROM asp_net_user_roles ur
                INNER JOIN asp_net_roles r ON r.id = ur.role_id
                WHERE ur.user_id = @UserId AND r.normalized_name = @Name
            )", new { UserId = userId, Name = normalizedRoleName });
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRoleName)
    {
        using var db = Connect();
        var users = await db.QueryAsync<ApplicationUser>(@"
            SELECT u.* FROM asp_net_users u
            INNER JOIN asp_net_user_roles ur ON u.id = ur.user_id
            INNER JOIN asp_net_roles r ON r.id = ur.role_id
            WHERE r.normalized_name = @Name", new { Name = normalizedRoleName });
        return users.ToList();
    }

    public async Task<IList<Claim>> GetClaimsAsync(string userId)
    {
        using var db = Connect();
        var rows = await db.QueryAsync<(string ClaimType, string ClaimValue)>(
            "SELECT claim_type, claim_value FROM asp_net_user_claims WHERE user_id = @UserId",
            new { UserId = userId });
        return rows.Select(r => new Claim(r.ClaimType, r.ClaimValue)).ToList();
    }

    public async Task AddClaimsAsync(string userId, IEnumerable<Claim> claims)
    {
        using var db = Connect();
        foreach (var claim in claims)
        {
            await db.ExecuteAsync(
                "INSERT INTO asp_net_user_claims (user_id, claim_type, claim_value) VALUES (@UserId, @Type, @Value)",
                new { UserId = userId, Type = claim.Type, Value = claim.Value });
        }
    }

    public async Task RemoveClaimsAsync(string userId, IEnumerable<Claim> claims)
    {
        using var db = Connect();
        foreach (var claim in claims)
        {
            await db.ExecuteAsync(
                "DELETE FROM asp_net_user_claims WHERE user_id = @UserId AND claim_type = @Type AND claim_value = @Value",
                new { UserId = userId, Type = claim.Type, Value = claim.Value });
        }
    }

    public async Task ReplaceClaimAsync(string userId, Claim claim, Claim newClaim)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            UPDATE asp_net_user_claims
            SET claim_type = @NewType, claim_value = @NewValue
            WHERE user_id = @UserId AND claim_type = @OldType AND claim_value = @OldValue",
            new { UserId = userId, NewType = newClaim.Type, NewValue = newClaim.Value, OldType = claim.Type, OldValue = claim.Value });
    }

    public async Task<IEnumerable<string>> GetRecoveryCodesAsync(string userId)
    {
        using var db = Connect();
        var value = await db.QuerySingleOrDefaultAsync<string?>(
            "SELECT value FROM asp_net_user_tokens WHERE user_id = @UserId AND login_provider = '[AspNetUserStore]' AND name = 'RecoveryCodes'",
            new { UserId = userId });
        if (string.IsNullOrEmpty(value)) return [];
        return value.Split(';');
    }

    public async Task ReplaceRecoveryCodesAsync(string userId, IEnumerable<string> codes)
    {
        using var db = Connect();
        var value = string.Join(';', codes);
        await db.ExecuteAsync(@"
            INSERT INTO asp_net_user_tokens (user_id, login_provider, name, value)
            VALUES (@UserId, '[AspNetUserStore]', 'RecoveryCodes', @Value)
            ON CONFLICT (user_id, login_provider, name)
            DO UPDATE SET value = @Value",
            new { UserId = userId, Value = value });
    }

    public async Task<bool> RedeemRecoveryCodeAsync(string userId, string code)
    {
        var codes = (await GetRecoveryCodesAsync(userId)).ToList();
        if (!codes.Remove(code)) return false;
        await ReplaceRecoveryCodesAsync(userId, codes);
        return true;
    }

    public async Task<int> CountRecoveryCodesAsync(string userId)
    {
        return (await GetRecoveryCodesAsync(userId)).Count();
    }

    public async Task<string?> GetTokenAsync(string userId, string loginProvider, string name)
    {
        using var db = Connect();
        return await db.QuerySingleOrDefaultAsync<string?>(
            "SELECT value FROM asp_net_user_tokens WHERE user_id = @UserId AND login_provider = @LoginProvider AND name = @Name",
            new { UserId = userId, LoginProvider = loginProvider, Name = name });
    }

    public async Task SetTokenAsync(string userId, string loginProvider, string name, string? value)
    {
        using var db = Connect();
        await db.ExecuteAsync(@"
            INSERT INTO asp_net_user_tokens (user_id, login_provider, name, value)
            VALUES (@UserId, @LoginProvider, @Name, @Value)
            ON CONFLICT (user_id, login_provider, name)
            DO UPDATE SET value = @Value",
            new { UserId = userId, LoginProvider = loginProvider, Name = name, Value = value });
    }

    public async Task RemoveTokenAsync(string userId, string loginProvider, string name)
    {
        using var db = Connect();
        await db.ExecuteAsync(
            "DELETE FROM asp_net_user_tokens WHERE user_id = @UserId AND login_provider = @LoginProvider AND name = @Name",
            new { UserId = userId, LoginProvider = loginProvider, Name = name });
    }
}
