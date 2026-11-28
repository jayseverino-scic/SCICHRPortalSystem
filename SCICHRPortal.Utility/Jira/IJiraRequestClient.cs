namespace SCICHRPortal.Utility.Jira;

public interface IJiraRequestClient
{
    Task<JiraRequestPage> GetPageAsync(JiraRequestType requestType, string? nextPageToken, CancellationToken cancellationToken);
}

public enum JiraRequestType
{
    Leave,
    OfficialBusiness
}

public sealed record JiraRequestRow(
    string Key, string Summary, string Status, string? EmployeeNo, string? EmployeeName,
    DateTimeOffset? From, DateTimeOffset? To);

public sealed record JiraRequestPage(IReadOnlyList<JiraRequestRow> Data, string? NextPageToken);
