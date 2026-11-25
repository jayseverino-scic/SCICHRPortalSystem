using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces;

public interface IEmployeeDailyAttendanceComputationService : IScopedService
{
    Task<EmployeeDailyAttendanceComputationPage> GetPageAsync(DateOnly attendanceDate, int projectId, int draw,
        int start, int length, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmployeeAttendanceDailyRow>> GetComputeRowsAsync(int projectId, DateOnly attendanceDate,
        CancellationToken cancellationToken);

    Task<bool> SaveApprovalEditsAsync(DateOnly attendanceDate, IReadOnlyList<AttendanceApprovalEdit> edits,
        ImportAudit audit, CancellationToken cancellationToken);
    Task<DailyAttendanceReimportResult?> ReimportAsync(int projectId, DateOnly attendanceDate, ImportAudit audit,
        CancellationToken cancellationToken);

    Task<DailyAttendanceReimportResult?> CompleteImportPhaseAsync(int projectId, DateOnly attendanceDate,
        AttendanceSessionPhase phase, ImportAudit audit, CancellationToken cancellationToken);
}
