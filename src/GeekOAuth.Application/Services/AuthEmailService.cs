using System.Net;
using System.Net.Mail;
using GeekOAuth.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GeekOAuth.Application.Services;

public sealed class AuthEmailService(IConfiguration config, IHostEnvironment environment) : IAuthEmailService
{
    private static readonly Serilog.ILogger Logger = Log.ForContext<AuthEmailService>();

    public Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default) =>
        SendAsync(email, "Confirm your Geek account", $"Confirm your email:\n\n{confirmationLink}", cancellationToken);

    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default) =>
        SendAsync(email, "Reset your Geek password", $"Reset your password:\n\n{resetLink}", cancellationToken);

    private async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment() || string.IsNullOrWhiteSpace(config["SMTP_HOST"]))
        {
            Logger.Information("Auth email (dev/log): To={To} Subject={Subject} Body={Body}", to, subject, body);
            return;
        }

        using var client = new SmtpClient(config["SMTP_HOST"], int.Parse(config["SMTP_PORT"] ?? "587", System.Globalization.CultureInfo.InvariantCulture))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(config["SMTP_USER"], config["SMTP_PASSWORD"]),
        };

        using var message = new MailMessage(config["SMTP_FROM"] ?? config["SMTP_USER"]!, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);
    }
}
