using Hangfire;
using Hangfire.Server;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.API.BackgroundJobs;

public sealed class NightlyAttendanceJobs(IAttendanceImportService attendance, IBackgroundJobClient jobs,
    ILogger<NightlyAttendanceJobs> logger)
{
    public const string RecurringId = "scic-nightly-attendance-import";
    public const string QueueName = "attendance-import";
    private static readonly ImportAudit ScheduledAudit = new("Scheduled Attendance Import", "Hangfire", "SCIC HR Portal");

    [AutomaticRetry(Attempts = 3)]
    public async Task DispatchAsync(PerformContext? context, CancellationToken cancellationToken)
    {
        // Hangfire records the scheduled occurrence even when strict catch-up enqueues it later.
        var timestamp = context?.GetJobParameter<long>("Time") ?? 0;
        if (timestamp <= 0) throw new InvalidOperationException("The scheduled attendance occurrence is missing.");

        var date = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(timestamp), PhilippineTime.Zone).Date.AddDays(-1);
        var followUp = date.AddDays(2);
        var followUpAt = new DateTimeOffset(followUp, PhilippineTime.Zone.GetUtcOffset(followUp));
        var projects = await attendance.GetScheduledProjectsAsync(cancellationToken);
        foreach (var project in projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            jobs.Enqueue<NightlyAttendanceJobs>(job => job.RunProjectAsync(project.Id, date, AttendanceSessionPhase.Daytime, CancellationToken.None));
            jobs.Schedule<NightlyAttendanceJobs>(
                job => job.RunProjectAsync(project.Id, date, AttendanceSessionPhase.Overnight, CancellationToken.None), followUpAt);
        }
        logger.LogInformation("Dispatched attendance imports for {BusinessDate} across {ProjectCount} projects; overnight jobs due {OvernightDue}",
            date, projects.Count, followUpAt);
    }

    [Queue(QueueName)]
    [AutomaticRetry(Attempts = 3)]
    public async Task RunProjectAsync(int projectId, DateTime businessDate, AttendanceSessionPhase phase, CancellationToken cancellationToken)
    {
        if (phase is not (AttendanceSessionPhase.Daytime or AttendanceSessionPhase.Overnight))
            throw new ArgumentOutOfRangeException(nameof(phase));

        logger.LogInformation("Starting scheduled attendance import for project {ProjectId}, {BusinessDate}, {Phase}",
            projectId, businessDate, phase);

        // A queued job may run after the project or device mapping has changed.
        var project = (await attendance.GetScheduledProjectsAsync(cancellationToken)).FirstOrDefault(row => row.Id == projectId);
        if (project?.Name is not { } name)
        {
            logger.LogWarning("Scheduled attendance import skipped unavailable mapping for project {ProjectId}, {BusinessDate}, {Phase}",
                projectId, businessDate, phase);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var through = businessDate.Date.AddDays(phase == AttendanceSessionPhase.Overnight ? 2 : 1);
        var punches = await attendance.ImportBiometricsAsync(businessDate.Date, through.AddTicks(-1), name, ScheduledAudit.Actor);
        logger.LogInformation(
            "Scheduled biometrics import for {ProjectName}, {BusinessDate}, {Phase} fetched {Fetched}, inserted {Inserted}, skipped {Skipped}, and failed {Failed}",
            name, businessDate, phase, punches.TotalFetched, punches.Inserted, punches.SkippedCount, punches.Failed);
        if (punches.Failed > 0)
            throw new InvalidOperationException($"Scheduled biometric import failed for project {projectId}. Failed punches: {punches.Failed}.");

        var timeLogs = await attendance.ImportTimeLogsAsync(businessDate.Date, businessDate.Date, name,
            ScheduledAudit.Actor, phase, ScheduledAudit, cancellationToken);
        if (timeLogs.ProjectUnavailable || timeLogs.Errors.Count > 0)
        {
            logger.LogWarning("Scheduled time log import for {ProjectName}, {BusinessDate}, {Phase} found {ValidationErrors} session errors; project unavailable {ProjectUnavailable}",
                name, businessDate, phase, timeLogs.Errors.Count, timeLogs.ProjectUnavailable);
            throw new InvalidOperationException($"Scheduled time log import could not validate project {projectId}. Review the project and its scheduled sessions.");
        }
        logger.LogInformation("Scheduled time log import for {ProjectName}, {BusinessDate}, {Phase} imported {Imported} and skipped {Skipped}",
            name, businessDate, phase, timeLogs.Imported, timeLogs.Skipped);
    }
}
