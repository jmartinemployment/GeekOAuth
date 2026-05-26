namespace GeekOAuth.Abstractions.Services;

public interface IAuthEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}
