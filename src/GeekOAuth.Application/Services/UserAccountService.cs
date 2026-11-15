using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Repositories;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Text;

namespace GeekOAuth.Application.Services;

public sealed class UserAccountService(
    UserManager<ApplicationUser> userManager,
    IUserRepository users,
    IAuthEmailService emailService,
    IConfiguration config) : IUserAccountService
{
    public Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default) =>
        CreateAsync(new ApplicationUser { UserName = email, Email = email, EmailConfirmed = false }, password, cancellationToken);

    public async Task<AccountOperationResult> ProvisionTenantOwnerAsync(Guid tenantId, string email, string password, string? displayName, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return new AccountOperationResult(false, "A tenant is required.");

        var tenant = tenantId.ToString("D");
        if (await users.FindByTenantAsync(tenant) is not null)
            return new AccountOperationResult(false, "That tenant already has an owner.", Conflict: true);

        if (await userManager.FindByEmailAsync(email) is not null)
            return new AccountOperationResult(false, "That email already has a login.", Conflict: true);

        var owner = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            Tenant = tenant,
        };
        return await CreateAsync(owner, password, cancellationToken);
    }

    private async Task<AccountOperationResult> CreateAsync(ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        var email = user.Email!;
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return new AccountOperationResult(false, string.Join("; ", result.Errors.Select(e => e.Description)));

        if (string.IsNullOrWhiteSpace(config["SMTP_HOST"]))
        {
            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, confirmToken);
            Log.Information("Email auto-confirmed for {Email} (SMTP_HOST not configured)", email);
        }
        else
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = BuildLink("Account/ConfirmEmail", "userId", user.Id, "code", token);
            await emailService.SendEmailConfirmationAsync(email, link, cancellationToken);
        }

        return new AccountOperationResult(true, UserId: user.Id);
    }

    public async Task<AccountOperationResult> ConfirmEmailAsync(string userId, string encodedCode, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return new AccountOperationResult(false, "Invalid confirmation link.");

        string code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedCode));
        }
        catch (FormatException)
        {
            return new AccountOperationResult(false, "Invalid confirmation link.");
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        return result.Succeeded
            ? new AccountOperationResult(true, UserId: user.Id)
            : new AccountOperationResult(false, "Invalid or expired confirmation link.");
    }

    public async Task<AccountOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return new AccountOperationResult(true);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = BuildLink("Account/ResetPassword", "email", email, "code", token);
        await emailService.SendPasswordResetAsync(email, link, cancellationToken);

        return new AccountOperationResult(true);
    }

    public async Task<AccountOperationResult> ResetPasswordAsync(
        string email,
        string encodedCode,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return new AccountOperationResult(false, "Invalid reset request.");

        string code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedCode));
        }
        catch (FormatException)
        {
            return new AccountOperationResult(false, "Invalid or expired reset link.");
        }

        var result = await userManager.ResetPasswordAsync(user, code, newPassword);
        return result.Succeeded
            ? new AccountOperationResult(true, UserId: user.Id)
            : new AccountOperationResult(false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private string BuildLink(string page, string idParam, string idValue, string codeParam, string codeValue)
    {
        var issuer = config["ISSUER_URL"]?.TrimEnd('/') ?? "https://localhost:5001";
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codeValue));
        return $"{issuer}/{page}?{idParam}={Uri.EscapeDataString(idValue)}&{codeParam}={encodedCode}";
    }
}
