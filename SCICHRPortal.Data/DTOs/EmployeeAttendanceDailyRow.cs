namespace SCICHRPortal.Data.DTOs;

public class EmployeeAttendanceDailyRow
{
    public int EmployeeAttendanceId { get; init; }
    public int TimeLogId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string? ProjectName { get; init; }
    public DateTime TimeIn { get; init; }
    public DateTime TimeOut { get; init; }
    public DateTime? ShiftStart { get; init; }
    public DateTime? ShiftEnd { get; init; }
    public bool IsFlexibleShift { get; init; }
    public bool IsNoShift { get; init; }
    public bool IsNoBreak { get; init; }
    public double ShiftHours { get; init; }
    public double RegularHour { get; init; }
    public double TotalLoggedHours { get; init; }
    public bool ApprovedOT { get; init; }
    public double OTHours { get; init; }
    public double NDHours { get; init; }
    public double ShiftUndertime { get; init; }
    public double ShiftLate { get; init; }
    public bool ApprovedHoliday { get; init; }
    public bool ApprovedHolidayOT { get; init; }
    public bool ApprovedSPHoliday { get; init; }
    public bool ApprovedSPHolidayOT { get; init; }
    public bool ApprovedRestDay { get; init; }
    public bool ApprovedRestDayOT { get; init; }
    public string? Department { get; init; }
}
