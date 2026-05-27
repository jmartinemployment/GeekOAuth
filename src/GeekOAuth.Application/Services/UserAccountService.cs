using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System.Text;

namespace GeekOAuth.Application.Services;

public sealed class UserAccountService(
    UserManager<ApplicationUser> userManager,
    IAuthEmailService emailService,
    IConfiguration config) : IUserAccountService
{
    public async Task<AccountOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
        };

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
