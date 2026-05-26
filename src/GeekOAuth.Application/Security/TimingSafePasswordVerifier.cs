using GeekOAuth.Abstractions.Models;
using Microsoft.AspNetCore.Identity;

namespace GeekOAuth.Application.Security;

/// <summary>
/// Holds a precomputed password hash used solely to normalize login timing when the email is unknown.
/// </summary>
public sealed class TimingSafePasswordVerifier(IPasswordHasher<ApplicationUser> passwordHasher)
{
    private readonly string _dummyHash = passwordHasher.HashPassword(
        new ApplicationUser { Id = Guid.Empty.ToString() },
        "GeekOAuth.TimingMitigation.Dummy!9x");

    public void RunDummyVerification(string password) =>
        passwordHasher.VerifyHashedPassword(
            new ApplicationUser { Id = Guid.Empty.ToString() },
            _dummyHash,
            password);
}
