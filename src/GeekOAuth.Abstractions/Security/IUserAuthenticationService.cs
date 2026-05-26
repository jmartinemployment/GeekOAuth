using GeekOAuth.Abstractions.Models;
using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Abstractions.Security;

public interface IUserAuthenticationService
{
    /// <summary>
    /// Validates credentials with constant-time behavior for unknown emails,
    /// lockout enforcement, and automatic password rehash on upgrade.
    /// </summary>
    Task<AuthenticationAttemptResult> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default);
}

public sealed record AuthenticationAttemptResult(
    SignInResult Result,
    ApplicationUser? User,
    bool PerformedDummyVerification);
