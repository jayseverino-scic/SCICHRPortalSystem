using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations;

public sealed class EmployeeDailyAttendanceComputationRepository(ApplicationContext context)
    : IEmployeeDailyAttendanceComputationRepository
{
    public async Task<EmployeeDailyAttendanceComputationPage> GetPageAsync(DateOnly attendanceDate, int projectId,
        int draw, int start, int length, CancellationToken cancellationToken)
    {
        var query = context.EmployeeDailyAttendanceComputation.AsNoTracking()
            .Where(row => row.AttendanceDate == attendanceDate && !row.Deleted);
        if (projectId != 0)
            query = query.Where(row => row.ProjectId == projectId);

        var count = await query.CountAsync(cancellationToken);
        var rows = await query.OrderBy(row => row.Employee!.LastName)
            .ThenBy(row => row.Employee!.FirstName)
            .ThenBy(row => row.TimeIn)
            .ThenBy(row => row.TimeLogId)
            .ThenBy(row => row.ProjectId)
            .Skip(start)
            .Take(length)
            .Select(row => new EmployeeDailyAttendanceComputationRow
            {
                AttendanceDate = row.AttendanceDate,
                ProjectId = row.ProjectId,
                ProjectName = row.Project!.Name,
                EmployeeAttendanceId = context.EmployeeAttendance
                    .Where(review => !review.Deleted && review.TimeLogId == row.TimeLogId)
                    .OrderByDescending(review => review.UpdatedAt ?? review.CreatedAt)
                    .ThenByDescending(review => review.EmployeeAttendanceId)
                    .Select(review => review.EmployeeAttendanceId)
                    .FirstOrDefault(),
                TimeLogId = row.TimeLogId,
                EmployeeId = row.EmployeeId,
                EmployeeNo = row.Employee!.EmployeeNo,
                EmployeeName = (row.Employee.LastName ?? "") + ", " + (row.Employee.FirstName ?? ""),
                Department = row.Employee.Department != null && !row.Employee.Department.Deleted
                    ? row.Employee.Department.DepartmentName : null,
                TimeIn = row.TimeIn,
                TimeOut = row.TimeOut,
                ShiftStart = row.ShiftStart,
                ShiftEnd = row.ShiftEnd,
                IsFlexibleShift = row.IsFlexibleShift,
                IsNoShift = row.IsNoShift,
                IsNoBreak = row.IsNoBreak,
                ShiftHours = row.ShiftHours,
                RegularHour = row.RegularHour,
                TotalLoggedHours = row.TotalLoggedHours,
                OTHours = row.OTHours,
                NDHours = row.NDHours,
                ShiftLate = row.ShiftLate,
                ShiftUndertime = row.ShiftUndertime,
                ApprovedOT = row.ApprovedOT,
                ApprovedHoliday = row.ApprovedHoliday,
                ApprovedHolidayOT = row.ApprovedHolidayOT,
                ApprovedSPHoliday = row.ApprovedSPHoliday,
                ApprovedSPHolidayOT = row.ApprovedSPHolidayOT,
                ApprovedRestDay = row.ApprovedRestDay,
                ApprovedRestDayOT = row.ApprovedRestDayOT
            }).ToListAsync(cancellationToken);

        return new EmployeeDailyAttendanceComputationPage(draw, count, count, rows);
    }

    public async Task<IReadOnlyList<EmployeeDailyAttendanceComputation>> GetForCutOffAsync(int projectId,
        DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var query = context.EmployeeDailyAttendanceComputation.AsNoTracking()
            .Include(row => row.Employee)
            .Where(row => !row.Deleted && row.AttendanceDate >= fromDate && row.AttendanceDate <= toDate);
        if (projectId != 0)
            query = query.Where(row => row.ProjectId == projectId);

        return await query.OrderBy(row => row.AttendanceDate)
            .ThenBy(row => row.TimeLogId)
            .ThenBy(row => row.ProjectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DailyAttendanceReimportResult?> SaveAsync(int projectId, DateOnly attendanceDate,
        ImportAudit audit, Func<EmployeeDailyAttendanceImportCheckpoint, bool>? recordPhase,
        Func<DailyAttendanceComputationSource, DailyAttendanceComputationBatch> compute,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await LockProjectDateAsync(projectId, attendanceDate, cancellationToken);

        bool projectActive = await context.Project.AnyAsync(project => project.Id == projectId && !project.Deleted,
            cancellationToken);
        if (!projectActive)
        {
            if (recordPhase != null)
                throw new InvalidOperationException($"Project {projectId} is no longer active.");

            return null;
        }

        if (recordPhase != null)
        {
            var checkpoint = await context.EmployeeDailyAttendanceImportCheckpoint
                .SingleOrDefaultAsync(row => row.ProjectId == projectId && row.AttendanceDate == attendanceDate,
                    cancellationToken);
            if (checkpoint == null)
            {
                checkpoint = new EmployeeDailyAttendanceImportCheckpoint
                {
                    ProjectId = projectId,
                    AttendanceDate = attendanceDate
                };
                context.EmployeeDailyAttendanceImportCheckpoint.Add(checkpoint);
            }

            if (!recordPhase(checkpoint))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await context.SaveChangesAsync(audit);
                await transaction.CommitAsync(cancellationToken);
                return null;
            }
        }

        var existing = await context.EmployeeDailyAttendanceComputation
            .Where(row => row.ProjectId == projectId && row.AttendanceDate == attendanceDate)
            .ToDictionaryAsync(row => row.TimeLogId, cancellationToken);
        var savedLogDeletionStates = existing.ToDictionary(entry => entry.Key, entry => entry.Value.Deleted);
        var source = await ReadSourceAsync(projectId, attendanceDate, savedLogDeletionStates, cancellationToken);
        var batch = compute(source);
        StageSavedRows(existing, batch.Rows, audit.Actor, DateTime.UtcNow);
        cancellationToken.ThrowIfCancellationRequested();
        await context.SaveChangesAsync(audit);
        await transaction.CommitAsync(cancellationToken);
        return new DailyAttendanceReimportResult(batch.Rows.Count, batch.SkippedLogs.Count, batch.SkippedLogs);
    }

    public async Task<bool> SaveApprovalEditsAsync(DateOnly attendanceDate, IReadOnlyList<AttendanceApprovalEdit> edits,
        ImportAudit audit, Func<int, DailyAttendanceComputationSource, IReadOnlyList<AttendanceApprovalEdit>,
            DailyAttendanceApprovalBatch?> compute, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var projectGroups = edits.GroupBy(edit => edit.ProjectId).OrderBy(group => group.Key).ToArray();
        var projectIds = projectGroups.Select(group => group.Key).ToArray();
        var activeProjectCount = await context.Project.CountAsync(project => projectIds.Contains(project.Id) &&
            !project.Deleted, cancellationToken);
        if (activeProjectCount != projectIds.Length)
            return false;

        var batches = new List<(Dictionary<int, EmployeeDailyAttendanceComputation> Existing, DailyAttendanceApprovalBatch Batch)>();
        foreach (var group in projectGroups)
        {
            var projectId = group.Key;
            await LockProjectDateAsync(projectId, attendanceDate, cancellationToken);

            var existing = await context.EmployeeDailyAttendanceComputation
                .Where(row => row.ProjectId == projectId && row.AttendanceDate == attendanceDate)
                .ToDictionaryAsync(row => row.TimeLogId, cancellationToken);
            var projectEdits = group.ToArray();
            var editedRows = new Dictionary<int, EmployeeDailyAttendanceComputation>(projectEdits.Length);
            foreach (var edit in projectEdits)
            {
                if (!existing.TryGetValue(edit.TimeLogId, out var row) || row.Deleted)
                    return false;

                editedRows.Add(edit.TimeLogId, row);
            }

            if (!await LockApprovalSourcesAsync(projectId, editedRows.Keys.ToArray(), cancellationToken))
                return false;

            // Include the full project/date source so Compute applies the same duplicate-session rules as Reimport.
            var savedLogDeletionStates = existing.ToDictionary(entry => entry.Key, entry => entry.Value.Deleted);
            var source = await ReadSourceAsync(projectId, attendanceDate, savedLogDeletionStates, cancellationToken);
            if (source.TimeLogs.Any(log => editedRows.TryGetValue(log.TimeLogId, out var row) &&
                row.EmployeeId != log.EmployeeId))
                return false;

            var batch = compute(projectId, source, projectEdits);
            if (batch is null)
                return false;

            batches.Add((editedRows, batch));
        }

        var approvals = batches.SelectMany(item => item.Batch.Approvals).ToArray();
        var updatedApprovalIds = approvals.Where(approval => approval.EmployeeAttendanceId != 0)
            .Select(approval => approval.EmployeeAttendanceId).ToArray();
        var existingApprovals = await context.EmployeeAttendance
            .Where(approval => !approval.Deleted && updatedApprovalIds.Contains(approval.EmployeeAttendanceId))
            .ToDictionaryAsync(approval => approval.EmployeeAttendanceId, cancellationToken);
        if (existingApprovals.Count != updatedApprovalIds.Length)
            return false;

        var now = DateTime.UtcNow;
        foreach (var approval in approvals)
        {
            if (approval.EmployeeAttendanceId == 0)
            {
                approval.CreatedAt = now;
                approval.CreatedBy = audit.Actor;
                context.EmployeeAttendance.Add(approval);
            }
            else
            {
                var existing = existingApprovals[approval.EmployeeAttendanceId];
                approval.CreatedAt = existing.CreatedAt;
                approval.CreatedBy = existing.CreatedBy;
                approval.UpdatedAt = now;
                approval.UpdatedBy = audit.Actor;
                context.Entry(existing).CurrentValues.SetValues(approval);
            }
        }

        foreach (var (existing, batch) in batches)
            StageSavedRows(existing, batch.Rows, audit.Actor, now);

        cancellationToken.ThrowIfCancellationRequested();
        await context.SaveChangesAsync(audit);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<bool> LockApprovalSourcesAsync(int projectId, int[] timeLogIds, CancellationToken cancellationToken)
    {
        // These IDs come from scoped saved rows. Keep ordinary time-log edits, employee transfers,
        // and project deactivation from changing the source during this save.
        var locked = await context.Database.SqlQuery<int>($"""
            SELECT log."TimeLogId" AS "Value"
            FROM "EmployeeTimeLog" AS log
            JOIN "Employee" AS employee ON employee."EmployeeId" = log."id"
            JOIN "Project" AS project ON project."Id" = employee."ProjectId"
            WHERE log."TimeLogId" = ANY({timeLogIds}) AND project."Id" = {projectId}
                AND NOT log."Deleted" AND NOT employee."Deleted" AND NOT project."Deleted"
            ORDER BY log."TimeLogId"
            FOR SHARE OF log, employee, project
            """).ToListAsync(cancellationToken);
        return locked.Count == timeLogIds.Length;
    }

    private Task LockProjectDateAsync(int projectId, DateOnly attendanceDate, CancellationToken cancellationToken) =>
        context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({projectId}, {attendanceDate.DayNumber})", cancellationToken);

    public async Task<DailyAttendanceComputationSource> GetComputeSourceAsync(int projectId, DateOnly attendanceDate,
        CancellationToken cancellationToken)
    {
        var savedRows = context.EmployeeDailyAttendanceComputation.AsNoTracking()
            .Where(row => row.AttendanceDate == attendanceDate && row.Employee!.ProjectId == row.ProjectId);
        if (projectId != 0)
            savedRows = savedRows.Where(row => row.ProjectId == projectId);

        var savedLogDeletionStates = await savedRows.ToDictionaryAsync(row => row.TimeLogId, row => row.Deleted,
            cancellationToken);
        return await ReadSourceAsync(projectId, attendanceDate, savedLogDeletionStates, cancellationToken,
            includeEmployee: true);
    }

    private async Task<DailyAttendanceComputationSource> ReadSourceAsync(int projectId, DateOnly attendanceDate,
        IReadOnlyDictionary<int, bool> savedLogDeletionStates, CancellationToken cancellationToken,
        bool includeEmployee = false)
    {
        var start = attendanceDate.ToDateTime(TimeOnly.MinValue);
        var logs = context.EmployeeTimeLog.AsNoTracking().Where(log => !log.Deleted && log.DateIn >= start);
        if (attendanceDate != DateOnly.MaxValue)
        {
            var end = start.AddDays(1);
            logs = logs.Where(log => log.DateIn < end);
        }

        if (includeEmployee)
            logs = logs.Include(log => log.Employee).ThenInclude(employee => employee!.Department)
                .Include(log => log.Employee).ThenInclude(employee => employee!.Project)
                .Where(log => log.Employee!.Project != null && !log.Employee.Project.Deleted);

        if (projectId != 0)
            logs = logs.Where(log => log.Employee!.ProjectId == projectId);

        var timeLogs = await logs.OrderBy(log => log.TimeLogId).ToListAsync(cancellationToken);

        if (timeLogs.Count == 0)
            return new DailyAttendanceComputationSource(timeLogs, [], [], new Dictionary<int, int>(), savedLogDeletionStates);

        var timeLogIds = timeLogs.Select(log => log.TimeLogId).ToArray();
        var approvals = await context.EmployeeAttendance.AsNoTracking()
            .Where(row => !row.Deleted && timeLogIds.Contains(row.TimeLogId))
            .ToListAsync(cancellationToken);

        var employeeIds = timeLogs.Select(log => log.EmployeeId).Distinct().ToArray();
        // Use the actual punch times for history lookup, including older logs whose DateIn and TimeIn differ.
        var firstPunch = timeLogs.Min(log => log.TimeIn) ?? start;
        var lastPunch = timeLogs.Max(log => log.TimeIn) ?? start;
        var periods = await context.EmployeeShift.AsNoTracking()
            .Where(row => !row.Deleted && employeeIds.Contains(row.EmployeeId) &&
                (row.EffectiveStartDate == null || row.EffectiveStartDate <= lastPunch) &&
                (row.EffectiveEndDate == null || row.EffectiveEndDate > firstPunch))
            .ToListAsync(cancellationToken);

        var shiftIds = periods.Select(period => period.ShiftId).Distinct().ToArray();
        var graceByShift = await context.Shift.AsNoTracking()
            .Where(shift => !shift.Deleted && shiftIds.Contains(shift.ShiftId))
            .ToDictionaryAsync(shift => shift.ShiftId, shift => shift.ShiftLateMinuteGracePeriod, cancellationToken);

        return new DailyAttendanceComputationSource(timeLogs, approvals, periods, graceByShift, savedLogDeletionStates);
    }

    private void StageSavedRows(Dictionary<int, EmployeeDailyAttendanceComputation> existing,
        IReadOnlyList<EmployeeDailyAttendanceComputation> rows, string actor, DateTime now)
    {
        foreach (var row in rows)
        {
            if (existing.Remove(row.TimeLogId, out var saved))
            {
                row.CreatedAt = saved.CreatedAt;
                row.CreatedBy = saved.CreatedBy;
                row.UpdatedAt = now;
                row.UpdatedBy = actor;
                context.Entry(saved).CurrentValues.SetValues(row);
            }
            else
            {
                row.CreatedAt = now;
                row.CreatedBy = actor;
                context.EmployeeDailyAttendanceComputation.Add(row);
            }
        }

        foreach (var stale in existing.Values.Where(row => !row.Deleted))
        {
            stale.Deleted = true;
            stale.UpdatedAt = now;
            stale.UpdatedBy = actor;
        }
    }
}
