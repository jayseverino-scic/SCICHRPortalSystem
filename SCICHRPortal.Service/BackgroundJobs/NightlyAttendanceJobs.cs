using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Service.Implementations;

namespace SCICHRPortal.Service.BackgroundJobs;

// Hangfire persists the job type and method signatures for scheduled work.
public sealed class NightlyAttendanceJobs(INightlyAttendanceImportService attendance, ILogger<NightlyAttendanceJobs> logger)
{
    public const string RecurringId = "scic-nightly-attendance-import";
    public const string QueueName = "attendance-import";

    [AutomaticRetry(Attempts = 3)]
    public async Task DispatchAsync(PerformContext? context, CancellationToken cancellationToken)
    {
        // Hangfire records the scheduled occurrence even when strict catch-up enqueues it later.
        var timestamp = context?.GetJobParameter<long>("Time") ?? 0;
        if (context?.Storage is not { } storage || timestamp <= 0)
            throw new InvalidOperationException("The scheduled attendance occurrence is missing.");

        var plan = await attendance.PlanDispatchAsync(DateTimeOffset.FromUnixTimeSeconds(timestamp), cancellationToken);
        foreach (var projectId in plan.ProjectIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            HangfireAttendanceDispatch.ScheduleProject(storage, context.BackgroundJob.Id, projectId, plan.BusinessDate, plan.OvernightDue);
        }

        logger.LogInformation("Attendance imports dispatched for {BusinessDate} across {ProjectCount} projects; overnight jobs due {OvernightDue}",
            plan.BusinessDate, plan.ProjectIds.Count, plan.OvernightDue);
    }

    [Queue(QueueName)]
    [AutomaticRetry(Attempts = 3)]
    public Task RunProjectAsync(int projectId, DateTime businessDate, AttendanceSessionPhase phase, CancellationToken cancellationToken) =>
        attendance.RunProjectAsync(projectId, businessDate, phase, cancellationToken);
}
