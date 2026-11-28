using SCICHRPortal.API.Controllers;
using SCICHRPortal.Utility.Jira;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.API.Extensions;

public static class JiraRequestsExtensions
{
    internal const string PolicyName = "ReadJiraRequests";

    public static IServiceCollection AddJiraRequests(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JiraSettings>(configuration.GetSection("Jira"));
        services.AddMemoryCache();
        services.AddHttpClient<IJiraRequestClient, JiraRequestClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.MaxResponseContentBufferSize = 1024 * 1024;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services.AddScoped<JiraRequestsPageReader>();
        services.AddDataProtection();
        services.AddAuthorizationBuilder().AddPolicy(PolicyName, policy =>
        {
            policy.RequireRole("Administrator");
            policy.RequireAssertion(context =>
                context.Resource is HttpContext httpContext &&
                !httpContext.RequestServices.GetRequiredService<ProjectDataScope>().Access.IsRestricted);
        });

        return services;
    }
}
