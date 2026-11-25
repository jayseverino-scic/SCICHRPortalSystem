namespace SCICHRPortal.Utility.Emails.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string html, CancellationToken cancellationToken = default);
}
