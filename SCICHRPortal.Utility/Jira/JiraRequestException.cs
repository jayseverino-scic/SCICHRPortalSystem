using System.Net;

namespace SCICHRPortal.Utility.Jira;

public sealed class JiraRequestException(JiraRequestFailure failure, HttpStatusCode? upstreamStatus = null)
    : Exception($"Jira request failed ({failure}).")
{
    public JiraRequestFailure Failure { get; } = failure;

    public HttpStatusCode? UpstreamStatus { get; } = upstreamStatus;
}

public enum JiraRequestFailure
{
    NotConfigured,
    InvalidCursor,
    RateLimited,
    TimedOut,
    Upstream
}
