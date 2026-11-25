using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SCICHRPortal.Utility.Emails;
using SCICHRPortal.Utility.Emails.Interfaces;
using SCICHRPortal.Utility.Emails.Models;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Extensions;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddPortalEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        services.AddOptions<EmailSettings>().Configure<IOptions<MailSettings>>((email, legacyOptions) =>
        {
            var legacy = legacyOptions.Value;
            email.MailServer = legacy.Host ?? "";
            email.MailPort = legacy.Port;
            email.SenderName = legacy.DisplayName ?? "";
            email.Sender = legacy.Mail ?? "";
            email.Username = legacy.Mail ?? "";
            email.Password = legacy.Password ?? "";
            configuration.GetSection("EmailSettings").Bind(email);
        });
        services.TryAddSingleton<Func<ISmtpClient>>(_ => () => new SmtpClient());
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }
}
