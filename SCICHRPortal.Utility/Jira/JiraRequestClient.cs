using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace SCICHRPortal.Utility.Jira;

public sealed class JiraRequestClient(HttpClient httpClient, IOptions<JiraSettings> settings, IMemoryCache cache)
    : IJiraRequestClient
{
    private const string EmployeeNoField = "customfield_10114";
    private const string FullNameField = "customfield_10119";
    private const string FirstNameField = "customfield_10380";
    private const string LastNameField = "customfield_10381";
    private const string StartDateTimeField = "customfield_10090";
    private const string EndDateTimeField = "customfield_10106";
    private static readonly string[] DateTimeFormats =
        ["yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'"];

    public async Task<JiraRequestPage> GetPageAsync(
        JiraRequestType requestType, string? nextPageToken, CancellationToken cancellationToken)
    {
        var apiToken = settings.Value.ApiToken?.Trim();
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            throw new JiraRequestException(JiraRequestFailure.NotConfigured);
        }

        var (requestTypeName, employeeNameFields) = requestType switch
        {
            JiraRequestType.Leave => ("Request for Leave", FullNameField),
            JiraRequestType.OfficialBusiness => ("Request for Official Business Schedule", $"{FirstNameField},{LastNameField}"),
            _ => throw new ArgumentOutOfRangeException(nameof(requestType))
        };

        var query = new Dictionary<string, string?>
        {
            ["jql"] = $"""project = HRSM AND "Request Type" = "{requestTypeName}" AND approvals = approved() AND status IN ("For HR Review", "Closed")""",
            ["fields"] = $"summary,status,{EmployeeNoField},{StartDateTimeField},{EndDateTimeField},{employeeNameFields}",
            ["maxResults"] = "50",
            ["nextPageToken"] = nextPageToken
        };

        try
        {
            var cloudId = await GetCloudIdAsync(cancellationToken);
            var url = QueryHelpers.AddQueryString($"https://api.atlassian.com/ex/jira/{cloudId:D}/rest/api/3/search/jql", query);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var username = settings.Value.Username?.Trim();
            request.Headers.Authorization = string.IsNullOrEmpty(username)
                ? new AuthenticationHeaderValue("Bearer", apiToken)
                : new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{apiToken}")));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var failure = response.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests => JiraRequestFailure.RateLimited,
                    HttpStatusCode.BadRequest when nextPageToken != null => JiraRequestFailure.InvalidCursor,
                    _ => JiraRequestFailure.Upstream
                };
                throw new JiraRequestException(failure, response.StatusCode);
            }

            var page = await response.Content.ReadFromJsonAsync<SearchResult>(cancellationToken);
            if (page?.Issues == null)
            {
                throw new JiraRequestException(JiraRequestFailure.Upstream);
            }

            var data = new List<JiraRequestRow>(page.Issues.Count);
            foreach (var issue in page.Issues)
            {
                if (string.IsNullOrWhiteSpace(issue?.Key) ||
                    issue.Fields?.Summary is not string summary ||
                    issue.Fields.Status?.Name is not string status)
                {
                    throw new JiraRequestException(JiraRequestFailure.Upstream);
                }

                var fields = issue.Fields;
                data.Add(new JiraRequestRow(issue.Key, summary, status, fields.GetEmployeeNo(), GetEmployeeName(requestType, fields),
                    ReadDateTime(fields.GetText(StartDateTimeField)), ReadDateTime(fields.GetText(EndDateTimeField))));
            }

            var continuation = page.IsLast == true ? null : page.NextPageToken;
            if (page.IsLast == false && string.IsNullOrWhiteSpace(continuation) ||
                continuation != null && continuation == nextPageToken)
            {
                throw new JiraRequestException(JiraRequestFailure.Upstream);
            }

            return new JiraRequestPage(data, continuation);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new JiraRequestException(JiraRequestFailure.TimedOut);
        }
        catch (HttpRequestException)
        {
            throw new JiraRequestException(JiraRequestFailure.Upstream);
        }
        catch (JsonException)
        {
            throw new JiraRequestException(JiraRequestFailure.Upstream);
        }
    }

    private static string? GetEmployeeName(JiraRequestType requestType, IssueFields fields)
    {
        if (requestType == JiraRequestType.Leave)
        {
            return fields.GetText(FullNameField);
        }

        var name = $"{fields.GetText(FirstNameField)} {fields.GetText(LastNameField)}".Trim();
        return name.Length == 0 ? null : name;
    }

    private static DateTimeOffset? ReadDateTime(string? value)
    {
        return DateTimeOffset.TryParseExact(value, DateTimeFormats, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var dateTime) ? dateTime : null;
    }

    private async Task<Guid> GetCloudIdAsync(CancellationToken cancellationToken)
    {
        var configuredId = settings.Value.CloudId;
        if (!string.IsNullOrWhiteSpace(configuredId))
        {
            if (Guid.TryParse(configuredId, out var cloudId) && cloudId != Guid.Empty)
            {
                return cloudId;
            }

            throw new JiraRequestException(JiraRequestFailure.NotConfigured);
        }

        var host = settings.Value.Host?.Trim().ToLowerInvariant();
        if (host == null || !host.EndsWith(".atlassian.net", StringComparison.Ordinal) ||
            Uri.CheckHostName(host) != UriHostNameType.Dns)
        {
            throw new JiraRequestException(JiraRequestFailure.NotConfigured);
        }

        var cacheKey = ("JiraCloudId", host);
        if (cache.TryGetValue<Guid>(cacheKey, out var cachedId))
        {
            return cachedId;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://{host}/_edge/tenant_info");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var failure = response.StatusCode == HttpStatusCode.TooManyRequests
                ? JiraRequestFailure.RateLimited
                : JiraRequestFailure.Upstream;
            throw new JiraRequestException(failure, response.StatusCode);
        }

        var tenant = await response.Content.ReadFromJsonAsync<TenantInfo>(cancellationToken);
        if (!Guid.TryParse(tenant?.CloudId, out var discoveredId) || discoveredId == Guid.Empty)
        {
            throw new JiraRequestException(JiraRequestFailure.Upstream);
        }

        cache.Set(cacheKey, discoveredId, TimeSpan.FromHours(24));
        return discoveredId;
    }

    private sealed record TenantInfo(string? CloudId);

    private sealed record SearchResult(List<JiraIssue?>? Issues, bool? IsLast, string? NextPageToken);

    private sealed record JiraIssue(string? Key, IssueFields? Fields);

    private sealed record IssueFields(string? Summary, IssueStatus? Status)
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? CustomFields { get; init; }

        public string? GetEmployeeNo()
        {
            if (CustomFields?.TryGetValue(EmployeeNoField, out var value) == true &&
                value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number) &&
                number == decimal.Truncate(number))
            {
                return number.ToString("0", CultureInfo.InvariantCulture);
            }

            return GetText(EmployeeNoField);
        }

        public string? GetText(string field)
        {
            if (CustomFields?.TryGetValue(field, out var value) != true || value.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var text = value.GetString()?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
    }

    private sealed record IssueStatus(string? Name);
}
