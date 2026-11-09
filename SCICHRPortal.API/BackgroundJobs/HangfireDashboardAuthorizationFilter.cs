using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace SCICHRPortal.API.BackgroundJobs;

internal sealed class HangfireDashboardAuthorizationFilter(string username, string password) : IDashboardAuthorizationFilter
{
    private readonly byte[] credentialHash = SHA256.HashData(Encoding.UTF8.GetBytes($"{username}:{password}"));

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        if (!httpContext.Request.IsHttps) return false;

        if (AuthenticationHeaderValue.TryParse(httpContext.Request.Headers.Authorization, out var authorization) &&
            authorization.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) &&
            authorization.Parameter is { } encodedCredentials &&
            MatchesCredentials(encodedCredentials))
        {
            return true;
        }

        httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"SCIC Hangfire\", charset=\"UTF-8\"";
        return false;
    }

    private bool MatchesCredentials(string encodedCredentials)
    {
        try
        {
            var credentials = Convert.FromBase64String(encodedCredentials);
            return CryptographicOperations.FixedTimeEquals(SHA256.HashData(credentials), credentialHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
