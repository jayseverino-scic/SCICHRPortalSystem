using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Helpers;

namespace SCICHRPortal.Service.Implementations
{
    public static class EmployeeShiftSessions
    {
        public static EmployeeShift? At(IEnumerable<EmployeeShift> periods, DateTime timestamp) => periods.SingleOrDefault(row =>
            !row.Deleted && (row.EffectiveStartDate == null || row.EffectiveStartDate <= timestamp) &&
            (row.EffectiveEndDate == null || timestamp < row.EffectiveEndDate));

        public static (DateTime? Start, DateTime? End) Window(EmployeeShift row, DateTime date)
        {
            var (start, end) = date.DayOfWeek switch
            {
                DayOfWeek.Monday => (row.MondayShiftStart, row.MondayShiftEnd),
                DayOfWeek.Tuesday => (row.TuesdayShiftStart, row.TuesdayShiftEnd),
                DayOfWeek.Wednesday => (row.WednesdayShiftStart, row.WednesdayShiftEnd),
                DayOfWeek.Thursday => (row.ThursdayShiftStart, row.ThursdayShiftEnd),
                DayOfWeek.Friday => (row.FridayShiftStart, row.FridayShiftEnd),
                DayOfWeek.Saturday => (row.SaturdayShiftStart, row.SaturdayShiftEnd),
                _ => (row.SundayShiftStart, row.SundayShiftEnd)
            };
            if (start == null || end == null) return (null, null);
            var from = PhilippineTime.SessionStart(date, start);
            var until = PhilippineTime.SessionStart(date, end);
            if (until <= from) until = until.AddDays(1);
            return (from, until);
        }

        public static (List<EmployeeTimeLog> Sessions, List<string> Errors) Build(int employeeId,
            IEnumerable<BiometricsLog> source, List<EmployeeShift> periods, DateTime from, DateTime until)
        {
            var sessions = new List<EmployeeTimeLog>();
            var errors = new List<string>();
            var punches = source.Where(row => row.Date.HasValue && row.Time.HasValue)
                .Select(row => (Timestamp: PhilippineTime.SessionStart(row.Date!.Value, row.Time), Row: row))
                .OrderBy(row => row.Timestamp).DistinctBy(row => row.Timestamp).ToList();
            var index = 0;
            while (index < punches.Count)
            {
                var first = punches[index];
                EmployeeShift? assignment;
                try { assignment = At(periods, first.Timestamp); }
                catch (InvalidOperationException)
                {
                    errors.Add($"Employee {employeeId}, {first.Timestamp:yyyy-MM-dd}: overlapping assignment periods.");
                    break;
                }
                if (assignment == null)
                {
                    if (first.Timestamp.Date >= from.Date && first.Timestamp.Date <= until.Date)
                        errors.Add($"Employee {employeeId}, {first.Timestamp:yyyy-MM-dd HH:mm}: no effective assignment.");
                    index++;
                    continue;
                }
                var window = Window(assignment, first.Timestamp.Date);
                var previous = Window(assignment, first.Timestamp.Date.AddDays(-1));
                if (previous.End >= first.Timestamp && previous.End.Value.Date > previous.Start!.Value.Date)
                {
                    // A checkout with no preceding clock-in is not a new overnight session.
                    if (first.Timestamp.Date >= from.Date && first.Timestamp.Date <= until.Date)
                        errors.Add($"Employee {employeeId}, {first.Timestamp:yyyy-MM-dd HH:mm}: overnight entry is missing; review the punches.");
                    index++;
                    continue;
                }
                if ((window.Start == null || window.End == null) && !assignment.IsNoShift && !assignment.IsFlexibleShift)
                {
                    if (first.Timestamp.Date >= from.Date && first.Timestamp.Date <= until.Date)
                        errors.Add($"Employee {employeeId}, {first.Timestamp:yyyy-MM-dd}: schedule times are incomplete.");
                    index++;
                    continue;
                }
                var overnight = window.End.HasValue && window.End.Value.Date > first.Timestamp.Date;
                var cutoff = first.Timestamp.Date.AddDays(overnight ? 2 : 1);
                DateTime? ownershipBoundary = null;
                DateTime? nextStart = null;
                // A later scheduled shift starts a separate session, including a
                // restoration after the assignment expires during overnight checkout.
                for (var day = 0; day <= 2; day++)
                {
                    foreach (var period in periods)
                    {
                        var nextWindow = Window(period, first.Timestamp.Date.AddDays(day));
                        if (nextWindow.Start > (window.Start ?? first.Timestamp) && nextWindow.Start > first.Timestamp &&
                            nextWindow.Start < cutoff && At(periods, nextWindow.Start.Value) == period &&
                            (!nextStart.HasValue || nextWindow.Start < nextStart))
                        {
                            nextStart = nextWindow.Start.Value;
                        }
                    }
                }
                if (nextStart.HasValue)
                {
                    var boundary = nextStart.Value;
                    // Partition adjacent session windows at the gap midpoint, also
                    // for recurring shifts. Choose the nearest start before splitting
                    // so database enumeration order cannot affect punch ownership.
                    var splitGap = window.End.HasValue && window.End.Value < boundary;
                    if (splitGap) boundary = window.End!.Value.AddTicks((boundary - window.End.Value).Ticks / 2);
                    if (boundary > first.Timestamp && boundary < cutoff)
                    {
                        cutoff = boundary;
                        ownershipBoundary = splitGap ? boundary : null;
                    }
                }
                var lastIndex = index;
                while (lastIndex + 1 < punches.Count && punches[lastIndex + 1].Timestamp < cutoff) lastIndex++;
                if (ownershipBoundary.HasValue && lastIndex + 1 < punches.Count && punches[lastIndex + 1].Timestamp == ownershipBoundary &&
                    punches[lastIndex + 1].Timestamp.Date >= from.Date && punches[lastIndex + 1].Timestamp.Date <= until.Date)
                    errors.Add($"Employee {employeeId}, {ownershipBoundary:yyyy-MM-dd HH:mm}: punch falls between sessions; review the pair.");
                var last = punches[lastIndex];
                if (lastIndex == index)
                {
                    if (first.Timestamp.Date >= from.Date && first.Timestamp.Date <= until.Date)
                        errors.Add($"Employee {employeeId}, {first.Timestamp:yyyy-MM-dd HH:mm}: a matching checkout is missing.");
                }
                else if (first.Timestamp.Date >= from.Date && first.Timestamp.Date <= until.Date)
                {
                    sessions.Add(new EmployeeTimeLog
                    {
                        EmployeeId = employeeId, DateIn = first.Timestamp.Date, DateOut = last.Timestamp.Date,
                        TimeIn = first.Timestamp, TimeOut = last.Timestamp,
                        ShiftStart = window.Start ?? first.Timestamp, ShiftEnd = window.End ?? last.Timestamp,
                        IsFlexibleShift = assignment.IsFlexibleShift, IsNoShift = assignment.IsNoShift, IsNoBreak = assignment.IsNoBreak,
                        ProjectTimeIn = TimeLogChanges.Optional(first.Row.ProjectName), ProjectTimeOut = TimeLogChanges.Optional(last.Row.ProjectName),
                        DeviceTimeIn = TimeLogChanges.Optional(first.Row.DeviceName), DeviceTimeOut = TimeLogChanges.Optional(last.Row.DeviceName),
                        SystemRemarks = first.Row.ImportSource == "File" || last.Row.ImportSource == "File" ? "File" : "Biometrics"
                    });
                }
                index = lastIndex + 1;
            }
            return (sessions, errors);
        }
    }
}
