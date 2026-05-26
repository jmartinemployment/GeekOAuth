using GeekOAuth.Abstractions.Models;
using GeekOAuth.Abstractions.Repositories;
using GeekOAuth.Abstractions.Security;
using Microsoft.AspNetCore.Identity;
namespace GeekOAuth.Application.Security;

public sealed class UserAuthenticationService(
    IUserRepository userRepository,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILookupNormalizer lookupNormalizer,
    TimingSafePasswordVerifier timingVerifier) : IUserAuthenticationService
{
    public async Task<AuthenticationAttemptResult> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = lookupNormalizer.NormalizeEmail(email);
        if (string.IsNullOrEmpty(normalizedEmail))
        {
            timingVerifier.RunDummyVerification(password);
            return new AuthenticationAttemptResult(SignInResult.Failed, null, true);
        }

        var user = await userRepository.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            timingVerifier.RunDummyVerification(password);
            return new AuthenticationAttemptResult(SignInResult.Failed, null, true);
        }

        if (!user.EmailConfirmed)
        {
            timingVerifier.RunDummyVerification(password);
            return new AuthenticationAttemptResult(SignInResult.NotAllowed, user, true);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new AuthenticationAttemptResult(SignInResult.LockedOut, user, false);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);

        return new AuthenticationAttemptResult(result, user, false);
    }
}
