namespace GeekOAuth.Abstractions.Services;

public interface IAuthEmailService
{
    /// <summary>
    /// False when this environment has no way to deliver mail: not Development and no SMTP server configured.
    /// Callers must refuse any flow whose only channel is email rather than issue a token nobody can receive.
    /// </summary>
    bool IsDeliveryAvailable { get; }

    Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}
