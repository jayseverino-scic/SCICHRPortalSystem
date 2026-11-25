using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SCICHRPortal.Utility.Emails.Interfaces;
using SCICHRPortal.Utility.Emails.Models;

namespace SCICHRPortal.Utility.Emails;

public sealed class EmailSender(IOptions<EmailSettings> settings, Func<ISmtpClient> createClient) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string html, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configuration = settings.Value;
        Validate(configuration);
        if (!MailboxAddress.TryParse(email, out var recipient))
            throw new ArgumentException("The email recipient is invalid.", nameof(email));
        if (configuration.ExcludedEmails?.Any(excluded =>
            string.Equals(excluded?.Trim(), recipient.Address, StringComparison.OrdinalIgnoreCase)) == true)
            throw new InvalidOperationException("Email delivery is excluded for this recipient.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(configuration.SenderName, configuration.Sender));
        message.To.Add(recipient);
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        // Open a fresh SMTP connection for each message.
        using var client = createClient();
        await client.ConnectAsync(configuration.MailServer, configuration.MailPort, SecureSocketOptions.StartTls, cancellationToken);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        await client.AuthenticateAsync(configuration.Username, configuration.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        // Transport failures propagate so account flows do not report false delivery success.
    }

    private static void Validate(EmailSettings configuration)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(configuration.MailServer)) errors.Add("EmailSettings.MailServer is required.");
        if (configuration.MailPort is < 1 or > 65535) errors.Add("EmailSettings.MailPort must be between 1 and 65535.");
        if (string.IsNullOrWhiteSpace(configuration.Sender) || !MailboxAddress.TryParse(configuration.Sender, out _))
            errors.Add("EmailSettings.Sender must be a valid email address.");
        if (string.IsNullOrWhiteSpace(configuration.Username)) errors.Add("EmailSettings.Username is required.");
        if (string.IsNullOrWhiteSpace(configuration.Password)) errors.Add("EmailSettings.Password is required.");
        if (errors.Count > 0) throw new OptionsValidationException(nameof(EmailSettings), typeof(EmailSettings), errors);
    }
}
