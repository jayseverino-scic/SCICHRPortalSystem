using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Service.Implementations;

static class HistoryDeletionChecks
{
    public static async Task RunAsync()
    {
        var deleteLatest = typeof(EmployeeShiftService).GetMethod("DeleteLatestFilteredAsync");
        var deleteRow = typeof(EmployeeShiftService).GetMethod("DeleteHistoryAsync");
        if (deleteLatest == null || deleteRow == null)
            throw new Exception("History deletion must support removing latest filtered rows and an exact employee history row.");
        var counter = new QueryCounter();
        using var db = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql($"Host=127.0.0.1;Port=55485;Database=scic_history_delete_check_{Guid.NewGuid():N};Username=postgres")
            .AddInterceptors(counter).Options, new SyntheticContext());
        using var xs = new XscribeContext(new DbContextOptions<XscribeContext>());
        using var tk = new TimekeepingContext(new DbContextOptions<TimekeepingContext>());
        var repo = new EmployeeShiftRepository(db, xs, tk);
        var service = new EmployeeShiftService(repo);
        var start = new DateTime(2026, 10, 16, 8, 0, 0);
        var passed = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            passed++; Console.WriteLine($"PASS: {message}");
        }
        Task<EmployeeShiftAssignmentResult> Latest(EmployeeShiftHistoryDeletionRequest request) =>
            (Task<EmployeeShiftAssignmentResult>)deleteLatest.Invoke(service, [request, "Synthetic deleting actor", CancellationToken.None])!;
        Task<bool> Delete(int employee, int row) =>
            (Task<bool>)deleteRow.Invoke(service, [employee, row, "Synthetic deleting actor", CancellationToken.None])!;
        void Employee(int id, string group = "Individual") => db.Employee.Add(new Employee { EmployeeId = id, EmployeeNo = $"QA{id}",
            FirstName = "Synthetic", LastName = group, Suffix = "", Email = "synthetic@example.invalid", ContactNumber = "", CreatedAt = DateTime.UtcNow });
        EmployeeShift Period(int id, DateTime? from, DateTime? until = null, bool temporary = false)
        {
            var row = new EmployeeShift { EmployeeId = id, ShiftId = 1, EffectiveStartDate = from, EffectiveEndDate = until,
                IsTemporary = temporary, ShiftDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CreatedBy = "Synthetic original actor",
                IsNoBreak = true, MondayShiftStart = start, MondayShiftEnd = start.AddHours(9) };
            db.EmployeeShift.Add(row); return row;
        }
        async Task<string> Snapshot(int employee) => JsonSerializer.Serialize(await db.EmployeeShift.AsNoTracking()
            .Where(row => row.EmployeeId == employee && !row.Deleted).OrderBy(row => row.AssignedShiftId).ToListAsync());
        await db.Database.EnsureCreatedAsync();
        try
        {
            db.Shift.Add(new Shift { ShiftId = 1, ShiftName = "Synthetic Schedule", CreatedAt = DateTime.UtcNow });
            foreach (var id in Enumerable.Range(1, 8)) Employee(id);
            var old = Period(1, null, start); var future = Period(1, start.AddDays(2));
            var current = Period(2, start.AddDays(-1));
            var baseline = Period(3, null, start); var temporary = Period(3, start, start.AddDays(1), true);
            var restored = Period(3, start.AddDays(1));
            var unknown = Period(4, null);
            for (var i = 0; i < 12; i++) Period(5, start.AddDays(i), start.AddDays(i + 1));
            Period(6, null, start); Period(6, start.AddDays(1));
            Period(7, null); db.Employee.Local.Single(row => row.EmployeeId == 7).Deleted = true;
            for (var i = 1; i <= 251; i++)
            {
                Employee(1000 + i, "Bulk"); Period(1000 + i, null, start); Period(1000 + i, start.AddDays(2));
                Employee(2000 + i, "Rollback"); Period(2000 + i, start);
            }
            db.EmployeeTimeLog.Add(new EmployeeTimeLog { TimeLogId = 1, EmployeeId = 2, DateIn = start.Date, DateOut = start.Date,
                TimeIn = start, TimeOut = start.AddHours(9), ShiftStart = start, ShiftEnd = start.AddHours(9), Version = 4,
                CreatedAt = DateTime.UtcNow, SystemRemarks = "Synthetic saved time log" });
            db.EmployeeAttendance.Add(new EmployeeAttendance { EmployeeAttendanceId = 1, EmployeeId = 2, TimeLogId = 1,
                TimeIn = start, TimeOut = start.AddHours(9), ShiftStart = start, ShiftEnd = start.AddHours(9),
                CreatedAt = DateTime.UtcNow, ShiftHours = 9 });
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            var logBefore = JsonSerializer.Serialize(await db.EmployeeTimeLog.AsNoTracking().SingleAsync());
            var attendanceBefore = JsonSerializer.Serialize(await db.EmployeeAttendance.AsNoTracking().SingleAsync());
            var created = (await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == future.AssignedShiftId)).CreatedAt;
            var result = await Latest(new() { SelectedEmployeeIds = [1] });
            Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 1,
                "Unassigned removes future latest history without effective dates.");
            var removed = await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == future.AssignedShiftId);
            var kept = await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == old.AssignedShiftId);
            Check(removed.Deleted && removed.CreatedAt == created && removed.CreatedBy == "Synthetic original actor" &&
                removed.UpdatedBy == "Synthetic deleting actor" && removed.UpdatedAt.HasValue && kept.EffectiveEndDate == start,
                "Deletion preserves original metadata, audits the actor and never extends another period.");
            Check(await repo.GetAtAsync(1, start.AddDays(3)) == null && (await repo.GetHistoryAsync(1, 0, 10)).Total == 1,
                "Deleted periods leave gaps and disappear from history and schedule lookups.");
            Check(await Delete(2, current.AssignedShiftId) && await repo.GetAtAsync(2, start) == null,
                "Deleting a current period makes its interval unassigned.");
            Check(await Delete(3, restored.AssignedShiftId) &&
                (await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == temporary.AssignedShiftId)).EffectiveEndDate == start.AddDays(1) &&
                await repo.GetAtAsync(3, start.AddDays(2)) == null, "Deleting automatic restoration leaves the temporary end unchanged.");
            Check(await Delete(3, temporary.AssignedShiftId) &&
                (await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == baseline.AssignedShiftId)).EffectiveEndDate == start,
                "Deleting a temporary period neither restores nor extends its predecessor.");
            Check((await Latest(new() { SelectedEmployeeIds = [4] })).Affected == 1 && (await repo.GetHistoryAsync(4, 0, 10)).Total == 0,
                "An unknown-start baseline can be removed when it is the latest row.");
            var olderPage = await repo.GetHistoryAsync(5, 10, 10);
            var olderId = olderPage.Data[0].AssignedShiftId;
            var survivor = olderPage.Data[1].AssignedShiftId;
            var beforeSurvivor = JsonSerializer.Serialize(await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == survivor));
            Check(await Delete(5, olderId) && (await repo.GetHistoryAsync(5, 0, 10)).Total == 11 &&
                JsonSerializer.Serialize(await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.AssignedShiftId == survivor)) == beforeSurvivor,
                "An older row from a later history page is deleted without changing neighboring records.");
            Check(!await Delete(6, survivor) && !await Delete(5, olderId), "Wrong employee IDs and already-deleted rows return missing without deleting another record.");
            Check(!await Delete(7, (await db.EmployeeShift.AsNoTracking().SingleAsync(row => row.EmployeeId == 7)).AssignedShiftId),
                "History of a soft-deleted employee cannot be changed.");
            result = await Latest(new() { SelectedEmployeeIds = [8] });
            Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 0, "Employees without history are skipped.");
            var auditCount = await db.Set<Audit>().CountAsync();
            result = await Latest(new() { ApplyToFilter = true, SearchKeyword = "Bulk", AsOf = start.AddDays(-1),
                FilterType = "Assigned", ExcludedEmployeeIds = [1001], SelectedEmployeeIds = [6] });
            Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 251 &&
                (await repo.GetHistoryAsync(1001, 0, 10)).Total == 2 && (await repo.GetHistoryAsync(1002, 0, 10)).Total == 1 &&
                (await repo.GetHistoryAsync(6, 0, 10)).Total == 1,
                "Filtered selection spans 251 employees, honors exclusions and includes selections outside the filter.");
            Check(await db.Set<Audit>().CountAsync() == auditCount + 251, "Every deleted row receives an audit record.");
            counter.Count = 0;
            result = await Latest(new() { ApplyToFilter = true, SearchKeyword = "Bulk", AsOf = start.AddDays(-1) });
            Check(result.Affected == 251 && counter.Count < 20, "Latest-row lookup is batched without per-employee queries.");
            counter.Count = 0;
            result = await Latest(new() { SelectedEmployeeIds = [] });
            Check(result.Status == EmployeeShiftAssignmentStatus.Invalid && counter.Count == 0, "Empty selection is rejected before database work.");
            Check((await Latest(new() { SelectedEmployeeIds = [6, 6] })).Status == EmployeeShiftAssignmentStatus.Invalid &&
                (await Latest(new() { SelectedEmployeeIds = [6], ExcludedEmployeeIds = [6] })).Status == EmployeeShiftAssignmentStatus.Invalid,
                "Duplicate or contradictory selections are invalid.");
            var before = await Snapshot(6);
            result = await Latest(new() { SelectedEmployeeIds = [6, 99999] });
            Check(result.Status == EmployeeShiftAssignmentStatus.Invalid && await Snapshot(6) == before,
                "An unavailable explicitly selected employee rejects the operation without partial deletion.");
            Check((await Latest(new() { ApplyToFilter = true, SearchKeyword = "NoMatch" })).Affected == 0,
                "Empty filtered selections succeed without modifying history.");
            Check(JsonSerializer.Serialize(await db.EmployeeTimeLog.AsNoTracking().SingleAsync()) == logBefore &&
                JsonSerializer.Serialize(await db.EmployeeAttendance.AsNoTracking().SingleAsync()) == attendanceBefore,
                "Stored time logs, versions and attendance remain unchanged after history deletion.");
            await db.Database.ExecuteSqlRawAsync("""
                CREATE FUNCTION synthetic_history_delete_fail() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                    IF NEW."Deleted" AND OLD."EmployeeId" = 2251 THEN
                        RAISE EXCEPTION 'Synthetic conflict' USING ERRCODE = '40001';
                    END IF;
                    RETURN NEW;
                END; $$;
                CREATE TRIGGER synthetic_history_delete_trigger BEFORE UPDATE ON "EmployeeShift"
                FOR EACH ROW EXECUTE FUNCTION synthetic_history_delete_fail();
                """);
            auditCount = await db.Set<Audit>().CountAsync();
            result = await Latest(new() { ApplyToFilter = true, SearchKeyword = "Rollback" });
            db.ChangeTracker.Clear();
            Check(result.Status == EmployeeShiftAssignmentStatus.Conflict &&
                await db.EmployeeShift.CountAsync(row => row.EmployeeId >= 2001 && row.Deleted) == 0 && await db.Set<Audit>().CountAsync() == auditCount,
                "A second-batch serialization conflict rolls back earlier deletions and audit entries.");
            Console.WriteLine($"PASS: {passed} PostgreSQL history deletion checks.");
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
}
