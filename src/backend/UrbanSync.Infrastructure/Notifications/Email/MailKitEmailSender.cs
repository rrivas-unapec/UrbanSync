using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UrbanSync.Application.Common.Interfaces.Notifications;

namespace UrbanSync.Infrastructure.Notifications.Email;

public sealed class MailKitEmailSender
    : IEmailSender
{
    private readonly SmtpEmailOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(
        IOptions<SmtpEmailOptions> options,
        ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "El envío de correos está deshabilitado. " +
                "No se enviará el mensaje a {RecipientEmail}.",
                recipientEmail);

            return false;
        }

        try
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _options.FromName,
                    _options.FromEmail));

            message.To.Add(
                new MailboxAddress(
                    recipientName,
                    recipientEmail));

            message.Subject = subject;

            message.Body =
                new BodyBuilder
                {
                    HtmlBody = htmlBody
                }
                .ToMessageBody();

            using var client = new SmtpClient();

            var socketOptions =
                _options.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                socketOptions,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(
                    _options.UserName))
            {
                await client.AuthenticateAsync(
                    _options.UserName,
                    _options.Password,
                    cancellationToken);
            }

            await client.SendAsync(
                message,
                cancellationToken);

            await client.DisconnectAsync(
                true,
                cancellationToken);

            _logger.LogInformation(
                "Correo enviado correctamente a {RecipientEmail}.",
                recipientEmail);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No fue posible enviar el correo a {RecipientEmail}.",
                recipientEmail);

            return false;
        }
    }
}