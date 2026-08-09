namespace UrbanSync.Application.Common.Interfaces.Notifications;

public interface IEmailSender
{
    Task<bool> SendAsync(
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}