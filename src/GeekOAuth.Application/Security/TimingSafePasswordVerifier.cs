using GeekOAuth.Abstractions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GeekOAuth.Application.Security;

/// <summary>
/// Holds a precomputed password hash used solely to normalize login timing when the email is unknown.
/// </summary>
public sealed class TimingSafePasswordVerifier
{
    private static readonly PasswordHasher<ApplicationUser> Verifier = new();
    private readonly string _dummyHash;

    public TimingSafePasswordVerifier(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        _dummyHash = passwordHasher.HashPassword(
            new ApplicationUser { Id = Guid.Empty.ToString() },
            "GeekOAuth.TimingMitigation.Dummy!9x");
    }

    public void RunDummyVerification(string password) =>
        Verifier.VerifyHashedPassword(
            new ApplicationUser { Id = Guid.Empty.ToString() },
            _dummyHash,
            password);
}
