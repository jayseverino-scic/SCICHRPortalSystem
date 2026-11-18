using System.Globalization;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using SCICHRPortal.Service.Implementations;

namespace SCICHRPortal.Service.BackgroundJobs;

internal static class HangfireAttendanceDispatch
{
    private static readonly IBackgroundJobFactory JobFactory = new BackgroundJobFactory();
    private static readonly TimeSpan DispatchHistoryRetention = TimeSpan.FromDays(30);

    public static void ScheduleProject(JobStorage storage, string dispatchId, int projectId, DateTime businessDate, DateTimeOffset overnightDue)
    {
        var key = $"scic-attendance-dispatch:{dispatchId}";
        var projectKey = projectId.ToString(CultureInfo.InvariantCulture);
        using var connection = (JobStorageConnection)storage.GetConnection();
        using var dispatchLock = connection.AcquireDistributedLock(key, TimeSpan.FromMinutes(1));
        if (connection.GetValueFromHash(key, projectKey) != null)
            return;

        var daytime = CreateProjectJob(storage, connection, projectId, businessDate, AttendanceSessionPhase.Daytime);
        var overnight = CreateProjectJob(storage, connection, projectId, businessDate, AttendanceSessionPhase.Overnight);

        // Commit both runnable states with the retry marker. Uninitialized job records expire if the commit fails.
        using var transaction = (JobStorageTransaction)connection.CreateWriteTransaction();
        JobFactory.StateMachine.ApplyState(new ApplyStateContext(storage, connection, transaction, daytime,
            new EnqueuedState(NightlyAttendanceJobs.QueueName), oldStateName: null));
        JobFactory.StateMachine.ApplyState(new ApplyStateContext(storage, connection, transaction, overnight,
            new ScheduledState(overnightDue.UtcDateTime), oldStateName: null));
        transaction.SetRangeInHash(key, [new KeyValuePair<string, string>(projectKey, daytime.Id)]);
        transaction.ExpireHash(key, DispatchHistoryRetention);
        transaction.Commit();
    }

    private static BackgroundJob CreateProjectJob(JobStorage storage, IStorageConnection connection, int projectId,
        DateTime businessDate, AttendanceSessionPhase phase)
    {
        var job = Job.FromExpression<NightlyAttendanceJobs>(target => target.RunProjectAsync(projectId, businessDate, phase, CancellationToken.None));
        return JobFactory.Create(new CreateContext(storage, connection, job, initialState: null))
            ?? throw new InvalidOperationException($"Hangfire could not create the {phase} attendance job for project {projectId}.");
    }
}
