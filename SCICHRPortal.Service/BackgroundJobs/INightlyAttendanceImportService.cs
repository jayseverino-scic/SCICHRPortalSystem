using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.BackgroundJobs;

public sealed record NightlyAttendancePlan(DateTime BusinessDate, DateTimeOffset OvernightDue, IReadOnlyList<int> ProjectIds);

public interface INightlyAttendanceImportService : IScopedService
{
    Task<NightlyAttendancePlan> PlanDispatchAsync(DateTimeOffset occurrence, CancellationToken cancellationToken);

    Task RunProjectAsync(int projectId, DateTime businessDate, AttendanceSessionPhase phase, CancellationToken cancellationToken);
}
