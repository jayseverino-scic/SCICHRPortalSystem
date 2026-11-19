namespace SCICHRPortal.Service.Implementations;

public sealed record EmployeeDailyAttendanceMetrics(
    double ShiftHours, double RegularHour, double TotalLoggedHours, double OTHours,
    double NDHours, double ShiftLate, double ShiftUndertime);

public static class EmployeeDailyAttendanceCalculator
{
    public static EmployeeDailyAttendanceMetrics Calculate(DateTime timeIn, DateTime timeOut,
        DateTime shiftStart, DateTime shiftEnd, double shiftLateGracePeriodMinutes, bool approvedOvertime)
    {
        var shiftHours = HoursBetween(shiftStart, shiftEnd);
        var loggedHours = HoursBetween(timeIn, timeOut);
        var lateHours = HoursBetween(shiftStart, timeIn);
        var lateMinutes = timeIn > shiftStart && lateHours > shiftLateGracePeriodMinutes / 60
            ? lateHours * 60
            : 0;
        var undertimeMinutes = timeOut < shiftEnd ? HoursBetween(timeOut, shiftEnd) * 60 : 0;

        // The existing computation uses the night window anchored to the check-in date.
        var nightStart = timeIn.Date.AddHours(22);
        var nightEnd = timeIn.Date.AddHours(30);
        var nightHours = timeIn <= nightEnd && timeOut >= nightStart
            ? HoursBetween(timeIn > nightStart ? timeIn : nightStart, timeOut < nightEnd ? timeOut : nightEnd)
            : 0;

        return new EmployeeDailyAttendanceMetrics(shiftHours, Math.Min(shiftHours, loggedHours), loggedHours,
            approvedOvertime ? loggedHours - shiftHours : 0, nightHours, lateMinutes, undertimeMinutes);
    }

    private static double HoursBetween(DateTime start, DateTime end) => (end - start).TotalMinutes / 60;
}
