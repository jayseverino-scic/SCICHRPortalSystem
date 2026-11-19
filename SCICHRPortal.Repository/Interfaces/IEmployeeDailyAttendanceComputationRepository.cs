using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces;

public interface IEmployeeDailyAttendanceComputationRepository : IScopedService
{
    Task<EmployeeDailyAttendanceComputationPage> GetPageAsync(DateOnly attendanceDate, int projectId, int draw,
        int start, int length, CancellationToken cancellationToken);
    Task<DailyAttendanceComputationSource> GetComputeSourceAsync(int projectId, DateOnly attendanceDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EmployeeDailyAttendanceComputation>> GetForCutOffAsync(int projectId,
        DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<DailyAttendanceReimportResult?> SaveAsync(int projectId, DateOnly attendanceDate, ImportAudit audit,
        Func<EmployeeDailyAttendanceImportCheckpoint, bool>? recordPhase,
        Func<DailyAttendanceComputationSource, DailyAttendanceComputationBatch> compute,
        CancellationToken cancellationToken);
}
