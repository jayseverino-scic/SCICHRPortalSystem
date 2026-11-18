using Hangfire;
using Hangfire.Common;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.Service.BackgroundJobs;

public static class NightlyAttendanceJobRegistration
{
    private const string StoredJobType = "SCICHRPortal.API.BackgroundJobs.NightlyAttendanceJobs, SCICHRPortal.API";

    // Keep the stored name so pending jobs and older workers can still address the job.
    public static IGlobalConfiguration UseNightlyAttendanceJobType(this IGlobalConfiguration configuration) =>
        configuration.UseTypeSerializer(type => type == typeof(NightlyAttendanceJobs)
                ? StoredJobType
                : TypeHelper.SimpleAssemblyTypeSerializer(type))
            .UseTypeResolver(name => name == StoredJobType || name.StartsWith(StoredJobType + ",", StringComparison.Ordinal)
                ? typeof(NightlyAttendanceJobs)
                : TypeHelper.DefaultTypeResolver(name));

    public static void Register(IRecurringJobManager recurringJobs)
    {
        recurringJobs.AddOrUpdate<NightlyAttendanceJobs>(
            NightlyAttendanceJobs.RecurringId,
            NightlyAttendanceJobs.QueueName,
            job => job.DispatchAsync(null, CancellationToken.None),
            Cron.Daily(),
            new RecurringJobOptions { TimeZone = PhilippineTime.Zone, MisfireHandling = MisfireHandlingMode.Strict });
    }
}
