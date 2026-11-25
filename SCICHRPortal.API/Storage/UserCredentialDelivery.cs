using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Storage;

public sealed class UserCredentialDelivery(IMailService mail)
    : IUserCredentialDelivery
{
    public async Task SendAsync(string email, string name, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var template = typeof(UserCredentialDelivery).Assembly.GetManifestResourceStream(
            "SCICHRPortal.API.EmailTemplates.NewPasswordTemplate.html")
            ?? throw new InvalidOperationException("The password email template is missing from the API assembly.");
        await mail.SendForgotPasswordEmailAsync(email, name, password, new FileStreamResult(template, "text/html"), cancellationToken);
    }
}
