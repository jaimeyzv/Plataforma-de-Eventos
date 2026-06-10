using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Notifications;

/// <summary>
/// Email sender built on MailKit. If SMTP is disabled (the default for the demo) it simply
/// logs the notification, so the pipeline works end-to-end without a real mail server.
/// </summary>
public sealed class SmtpNotificationSender : INotificationSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpNotificationSender> _logger;

    public SmtpNotificationSender(IOptions<EmailSettings> settings, ILogger<SmtpNotificationSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(NotificationLog notification, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "[SIMULATED EMAIL] To: {Recipient} | Subject: {Subject} | Body: {Body}",
                notification.Recipient, notification.Subject, notification.Body);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(notification.Recipient));
        message.Subject = notification.Subject;
        message.Body = new TextPart("plain") { Text = notification.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Recipient} for event {EventId}.", notification.Recipient, notification.EventId);
    }
}
