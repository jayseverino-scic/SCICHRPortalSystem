using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Utility.Jira;

namespace SCICHRPortal.API.Controllers;

public sealed class JiraRequestsPageReader(
    IJiraRequestClient jiraClient,
    IDataProtectionProvider dataProtection,
    ILogger<JiraRequestsPageReader> logger)
{
    private const int MaximumCursorLength = 4096;
    private const string CursorError = "This page is no longer available. Reload approved requests to start again.";

    internal async Task<IActionResult> GetPageAsync(
        JiraRequestType requestType, string? cursor, CancellationToken cancellationToken)
    {
        if (cursor?.Length > MaximumCursorLength)
        {
            return new BadRequestObjectResult(new { Message = CursorError });
        }

        var protector = dataProtection.CreateProtector("SCICHRPortal.JiraRequests.Paging.v1", requestType.ToString());
        string? nextPageToken;
        try
        {
            nextPageToken = cursor == null ? null : protector.Unprotect(cursor);
        }
        catch (Exception error) when (error is CryptographicException or FormatException)
        {
            return new BadRequestObjectResult(new { Message = CursorError });
        }

        try
        {
            var page = await jiraClient.GetPageAsync(requestType, nextPageToken, cancellationToken);
            var nextCursor = page.NextPageToken == null ? null : protector.Protect(page.NextPageToken);
            if (nextCursor?.Length > MaximumCursorLength)
            {
                logger.LogWarning("Jira returned an oversized continuation for {JiraRequestType} requests.", requestType);
                return ErrorResponse(StatusCodes.Status502BadGateway,
                    "The next page of approved requests is unavailable. Please try again.");
            }

            return new OkObjectResult(new { page.Data, NextCursor = nextCursor });
        }
        catch (JiraRequestException error)
        {
            logger.LogWarning("Could not load {JiraRequestType} requests from Jira. Failure: {Failure}. Jira status: {JiraStatus}.",
                requestType, error.Failure, error.UpstreamStatus);

            return error.Failure switch
            {
                JiraRequestFailure.InvalidCursor => new BadRequestObjectResult(new { Message = CursorError }),
                JiraRequestFailure.NotConfigured => ErrorResponse(StatusCodes.Status503ServiceUnavailable,
                    "Approved requests are unavailable because the connection has not been configured."),
                JiraRequestFailure.RateLimited => ErrorResponse(StatusCodes.Status503ServiceUnavailable,
                    "The request service is busy. Please wait and try again."),
                JiraRequestFailure.TimedOut => ErrorResponse(StatusCodes.Status504GatewayTimeout,
                    "The request service took too long to respond. Please try again."),
                _ => ErrorResponse(StatusCodes.Status502BadGateway,
                    "Approved requests could not be loaded. Please try again.")
            };
        }
    }

    private static ObjectResult ErrorResponse(int statusCode, string message)
    {
        return new ObjectResult(new { Message = message }) { StatusCode = statusCode };
    }
}
