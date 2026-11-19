using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.DTOs;

public sealed record EmployeeDailyAttendanceComputationPage(
    int Draw, int RecordsTotal, int RecordsFiltered, IReadOnlyList<EmployeeDailyAttendanceComputationRow> Data);

public sealed class EmployeeDailyAttendanceComputationRow
{
    public DateOnly AttendanceDate { get; init; }
    public int ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public int EmployeeAttendanceId { get; init; }
    public int TimeLogId { get; init; }
    public int EmployeeId { get; init; }
    public string? EmployeeNo { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string? Department { get; init; }
    public DateTime TimeIn { get; init; }
    public DateTime TimeOut { get; init; }
    public DateTime ShiftStart { get; init; }
    public DateTime ShiftEnd { get; init; }
    public bool IsFlexibleShift { get; init; }
    public bool IsNoShift { get; init; }
    public bool IsNoBreak { get; init; }
    public double ShiftHours { get; init; }
    public double RegularHour { get; init; }
    public double TotalLoggedHours { get; init; }
    public double OTHours { get; init; }
    public double NDHours { get; init; }
    public double ShiftLate { get; init; }
    public double ShiftUndertime { get; init; }
    public bool ApprovedOT { get; init; }
    public bool ApprovedHoliday { get; init; }
    public bool ApprovedHolidayOT { get; init; }
    public bool ApprovedSPHoliday { get; init; }
    public bool ApprovedSPHolidayOT { get; init; }
    public bool ApprovedRestDay { get; init; }
    public bool ApprovedRestDayOT { get; init; }
}

public sealed record DailyAttendanceSkippedLog(int TimeLogId, string Reason);

public sealed record DailyAttendanceReimportResult(
    int SavedCount, int SkippedCount, IReadOnlyList<DailyAttendanceSkippedLog> SkippedLogs);

public sealed record DailyAttendanceComputationSource(
    IReadOnlyList<EmployeeTimeLog> TimeLogs,
    IReadOnlyList<EmployeeAttendance> Approvals,
    IReadOnlyList<EmployeeShift> ShiftPeriods,
    IReadOnlyDictionary<int, int> GracePeriodMinutesByShiftId,
    IReadOnlyDictionary<int, bool> SavedLogDeletionStates);

public sealed record DailyAttendanceComputationBatch(
    IReadOnlyList<EmployeeDailyAttendanceComputation> Rows, IReadOnlyList<DailyAttendanceSkippedLog> SkippedLogs);
