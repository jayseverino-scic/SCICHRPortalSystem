using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations;

public sealed class EmployeeDailyAttendanceComputationService(IEmployeeDailyAttendanceComputationRepository repository)
    : IEmployeeDailyAttendanceComputationService
{
    public Task<EmployeeDailyAttendanceComputationPage> GetPageAsync(DateOnly attendanceDate, int projectId, int draw,
        int start, int length, CancellationToken cancellationToken) =>
        repository.GetPageAsync(attendanceDate, projectId, draw, start, length, cancellationToken);

    public async Task<IReadOnlyList<EmployeeAttendanceDailyRow>> GetComputeRowsAsync(int projectId,
        DateOnly attendanceDate, CancellationToken cancellationToken)
    {
        var source = await repository.GetComputeSourceAsync(projectId, attendanceDate, cancellationToken);
        var (batch, approvals) = Compute(projectId, attendanceDate, source, cancellationToken);
        var logsById = source.TimeLogs.ToDictionary(log => log.TimeLogId);
        return batch.Rows.Select(row => ToDailyRow(row, logsById[row.TimeLogId],
            approvals.GetValueOrDefault(row.TimeLogId)?.EmployeeAttendanceId ?? 0)).ToList();
    }

    private static EmployeeAttendanceDailyRow ToDailyRow(EmployeeDailyAttendanceComputation attendance, EmployeeTimeLog log, int employeeAttendanceId)
    {
        return new EmployeeAttendanceDailyRow
        {
            EmployeeAttendanceId = employeeAttendanceId,
            TimeLogId = attendance.TimeLogId,
            EmployeeId = attendance.EmployeeId,
            EmployeeName = $"{log.Employee?.LastName}, {log.Employee?.FirstName}",
            ProjectName = log.Employee?.Project?.Name,
            TimeIn = attendance.TimeIn,
            TimeOut = attendance.TimeOut,
            ShiftStart = attendance.ShiftStart,
            ShiftEnd = attendance.ShiftEnd,
            IsFlexibleShift = attendance.IsFlexibleShift,
            IsNoShift = attendance.IsNoShift,
            IsNoBreak = attendance.IsNoBreak,
            ShiftHours = attendance.ShiftHours,
            RegularHour = attendance.RegularHour,
            TotalLoggedHours = attendance.TotalLoggedHours,
            ApprovedOT = attendance.ApprovedOT,
            OTHours = attendance.OTHours,
            NDHours = attendance.NDHours,
            ShiftUndertime = attendance.ShiftUndertime,
            ShiftLate = attendance.ShiftLate,
            ApprovedHoliday = attendance.ApprovedHoliday,
            ApprovedHolidayOT = attendance.ApprovedHolidayOT,
            ApprovedSPHoliday = attendance.ApprovedSPHoliday,
            ApprovedSPHolidayOT = attendance.ApprovedSPHolidayOT,
            ApprovedRestDay = attendance.ApprovedRestDay,
            ApprovedRestDayOT = attendance.ApprovedRestDayOT,
            Department = log.Employee?.Department is { Deleted: false } department ? department.DepartmentName : null
        };
    }

    public Task<bool> SaveApprovalEditsAsync(DateOnly attendanceDate, IReadOnlyList<AttendanceApprovalEdit> edits,
        ImportAudit audit, CancellationToken cancellationToken) =>
        repository.SaveApprovalEditsAsync(attendanceDate, edits, audit,
            (projectId, source, projectEdits) => ComputeApprovalEdits(projectId, attendanceDate, source, projectEdits,
                cancellationToken), cancellationToken);

    private static DailyAttendanceApprovalBatch? ComputeApprovalEdits(int projectId, DateOnly attendanceDate,
        DailyAttendanceComputationSource source, IReadOnlyList<AttendanceApprovalEdit> edits,
        CancellationToken cancellationToken)
    {
        var currentApprovals = GetLatestApprovals(source.Approvals);
        var overrides = edits.ToDictionary(edit => edit.TimeLogId,
            edit => ApplyApprovalEdit(edit, currentApprovals.GetValueOrDefault(edit.TimeLogId)));
        var updatedSource = source with
        {
            Approvals = source.Approvals.Where(approval => !overrides.ContainsKey(approval.TimeLogId))
                .Concat(overrides.Values).ToArray()
        };
        var (batch, _) = Compute(projectId, attendanceDate, updatedSource, cancellationToken);
        var editedRows = batch.Rows.Where(row => overrides.ContainsKey(row.TimeLogId)).ToArray();
        if (editedRows.Length != edits.Count)
            return null;

        foreach (var row in editedRows)
        {
            var approval = overrides[row.TimeLogId];
            approval.EmployeeId = row.EmployeeId;
            approval.TimeIn = row.TimeIn;
            approval.TimeOut = row.TimeOut;
            approval.ShiftStart = row.ShiftStart;
            approval.ShiftEnd = row.ShiftEnd;
            approval.IsFlexibleShift = row.IsFlexibleShift;
            approval.IsNoShift = row.IsNoShift;
            approval.IsNoBreak = row.IsNoBreak;
            approval.ShiftHours = row.ShiftHours;
            approval.RegularHour = row.RegularHour;
            approval.TotalLoggedHours = row.TotalLoggedHours;
            approval.OTHours = row.OTHours;
            approval.NDHours = row.NDHours;
            approval.ShiftLate = row.ShiftLate;
            approval.ShiftUndertime = row.ShiftUndertime;
        }

        return new DailyAttendanceApprovalBatch(overrides.Values.ToArray(), editedRows);
    }

    private static EmployeeAttendance ApplyApprovalEdit(AttendanceApprovalEdit edit, EmployeeAttendance? current) => new()
    {
        EmployeeAttendanceId = current?.EmployeeAttendanceId ?? 0,
        TimeLogId = edit.TimeLogId,
        CreatedAt = current?.CreatedAt ?? default,
        UpdatedAt = current?.UpdatedAt,
        ApprovedOT = edit.ApprovedOT ?? current?.ApprovedOT ?? false,
        ApprovedHoliday = edit.ApprovedHoliday ?? current?.ApprovedHoliday ?? false,
        ApprovedHolidayOT = edit.ApprovedHolidayOT ?? current?.ApprovedHolidayOT ?? false,
        ApprovedSPHoliday = edit.ApprovedSPHoliday ?? current?.ApprovedSPHoliday ?? false,
        ApprovedSPHolidayOT = edit.ApprovedSPHolidayOT ?? current?.ApprovedSPHolidayOT ?? false,
        ApprovedRestDay = edit.ApprovedRestDay ?? current?.ApprovedRestDay ?? false,
        ApprovedRestDayOT = edit.ApprovedRestDayOT ?? current?.ApprovedRestDayOT ?? false
    };

    public Task<DailyAttendanceReimportResult?> ReimportAsync(int projectId, DateOnly attendanceDate, ImportAudit audit,
        CancellationToken cancellationToken) =>
        repository.SaveAsync(projectId, attendanceDate, audit, null,
            source => Compute(projectId, attendanceDate, source, cancellationToken).Batch, cancellationToken);

    public Task<DailyAttendanceReimportResult?> CompleteImportPhaseAsync(int projectId, DateOnly attendanceDate,
        AttendanceSessionPhase phase, ImportAudit audit, CancellationToken cancellationToken)
    {
        if (phase is not (AttendanceSessionPhase.Daytime or AttendanceSessionPhase.Overnight))
            throw new ArgumentOutOfRangeException(nameof(phase));

        return repository.SaveAsync(projectId, attendanceDate, audit, checkpoint =>
        {
            if (phase == AttendanceSessionPhase.Daytime)
                checkpoint.DaytimeCompleted = true;
            else
                checkpoint.OvernightCompleted = true;

            return checkpoint.DaytimeCompleted && checkpoint.OvernightCompleted;
        }, source => Compute(projectId, attendanceDate, source, cancellationToken).Batch, cancellationToken);
    }

    private static (DailyAttendanceComputationBatch Batch, Dictionary<int, EmployeeAttendance> Approvals) Compute(
        int projectId, DateOnly attendanceDate,
        DailyAttendanceComputationSource source, CancellationToken cancellationToken)
    {
        var approvalsByLog = GetLatestApprovals(source.Approvals);
        var periodsByEmployee = source.ShiftPeriods.ToLookup(row => row.EmployeeId);
        var rows = new List<EmployeeDailyAttendanceComputation>(source.TimeLogs.Count);
        var skipped = new List<DailyAttendanceSkippedLog>();

        foreach (var log in source.TimeLogs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (log.TimeIn is not { } timeIn || log.TimeOut is not { } timeOut)
            {
                skipped.Add(new DailyAttendanceSkippedLog(log.TimeLogId, "A check-in or checkout is missing."));
                continue;
            }

            if (log.ShiftStart is not { } shiftStart || log.ShiftEnd is not { } shiftEnd)
            {
                skipped.Add(new DailyAttendanceSkippedLog(log.TimeLogId, "A shift start or end is missing."));
                continue;
            }

            if (timeOut <= timeIn || shiftEnd <= shiftStart)
            {
                skipped.Add(new DailyAttendanceSkippedLog(log.TimeLogId, "Checkout and shift end must follow their start times."));
                continue;
            }

            EmployeeShift? shift;
            try
            {
                shift = EmployeeShiftSessions.At(periodsByEmployee[log.EmployeeId], timeIn);
            }
            catch (InvalidOperationException)
            {
                skipped.Add(new DailyAttendanceSkippedLog(log.TimeLogId, "The employee has overlapping shift assignments."));
                continue;
            }

            var gracePeriod = shift != null ? source.GracePeriodMinutesByShiftId.GetValueOrDefault(shift.ShiftId) : 0;
            var approval = approvalsByLog.GetValueOrDefault(log.TimeLogId);
            var metrics = EmployeeDailyAttendanceCalculator.Calculate(timeIn, timeOut, shiftStart, shiftEnd,
                gracePeriod, approval?.ApprovedOT == true);

            rows.Add(new EmployeeDailyAttendanceComputation
            {
                ProjectId = log.Employee?.ProjectId ?? projectId,
                AttendanceDate = attendanceDate,
                TimeLogId = log.TimeLogId,
                EmployeeId = log.EmployeeId,
                TimeIn = timeIn,
                TimeOut = timeOut,
                ShiftStart = shiftStart,
                ShiftEnd = shiftEnd,
                IsFlexibleShift = log.IsFlexibleShift,
                IsNoShift = log.IsNoShift,
                IsNoBreak = log.IsNoBreak,
                ShiftHours = metrics.ShiftHours,
                RegularHour = metrics.RegularHour,
                TotalLoggedHours = metrics.TotalLoggedHours,
                OTHours = metrics.OTHours,
                NDHours = metrics.NDHours,
                ShiftLate = metrics.ShiftLate,
                ShiftUndertime = metrics.ShiftUndertime,
                ApprovedOT = approval?.ApprovedOT == true,
                ApprovedHoliday = approval?.ApprovedHoliday == true,
                ApprovedHolidayOT = approval?.ApprovedHolidayOT == true,
                ApprovedSPHoliday = approval?.ApprovedSPHoliday == true,
                ApprovedSPHolidayOT = approval?.ApprovedSPHolidayOT == true,
                ApprovedRestDay = approval?.ApprovedRestDay == true,
                ApprovedRestDayOT = approval?.ApprovedRestDayOT == true
            });
        }

        var uniqueRows = new List<EmployeeDailyAttendanceComputation>(rows.Count);
        foreach (var group in rows.GroupBy(row => (row.ProjectId, row.EmployeeId, row.TimeIn, row.TimeOut)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Keep the surviving row's identity on reimport, using the newest review to break ties.
            var retained = group.OrderByDescending(row => GetSavedLogPriority(source.SavedLogDeletionStates, row.TimeLogId))
                .ThenByDescending(row => approvalsByLog.TryGetValue(row.TimeLogId, out var approval)
                    ? approval.UpdatedAt ?? approval.CreatedAt
                    : DateTime.MinValue)
                .ThenBy(row => row.TimeLogId)
                .First();
            uniqueRows.Add(retained);

            foreach (var duplicate in group.Where(row => row.TimeLogId != retained.TimeLogId))
                skipped.Add(new DailyAttendanceSkippedLog(duplicate.TimeLogId,
                    $"Identical punches are already covered by time log {retained.TimeLogId}."));
        }

        return (new DailyAttendanceComputationBatch(uniqueRows, skipped), approvalsByLog);
    }

    private static Dictionary<int, EmployeeAttendance> GetLatestApprovals(IEnumerable<EmployeeAttendance> approvals) =>
        approvals.GroupBy(row => row.TimeLogId).ToDictionary(group => group.Key,
            group => group.OrderByDescending(row => row.UpdatedAt ?? row.CreatedAt)
                .ThenByDescending(row => row.EmployeeAttendanceId).First());

    private static int GetSavedLogPriority(IReadOnlyDictionary<int, bool> savedLogDeletionStates, int timeLogId)
    {
        if (!savedLogDeletionStates.TryGetValue(timeLogId, out var deleted))
            return 0;

        return deleted ? 1 : 2;
    }
}
