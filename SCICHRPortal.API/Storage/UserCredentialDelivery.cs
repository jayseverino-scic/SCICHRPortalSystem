using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Storage;

public sealed class UserCredentialDelivery(IHttpClientFactory clients, IMailService mail, IOptions<AppSettings> settings)
    : IUserCredentialDelivery
{
    public async Task SendAsync(string email, string name, string password, CancellationToken cancellationToken)
    {
        var templateUrl = new Uri(new Uri(settings.Value.WebUrl!), "html/templates/NewPasswordTemplate.html");
        using var client = clients.CreateClient(nameof(UserCredentialDelivery));
        using var response = await client.GetAsync(templateUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;
        await mail.SendForgotPasswordEmailAsync(email, name, password, new FileStreamResult(copy, "text/html"));
    }
}
