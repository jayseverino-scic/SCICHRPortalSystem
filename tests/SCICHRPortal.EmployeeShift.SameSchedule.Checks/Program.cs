using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Utility.HttpContext.Interfaces;

if (args.Contains("--history-deletion"))
{
    await HistoryDeletionChecks.RunAsync();
    return;
}

// This executable owns a fresh disposable database on the isolated test server.
// It never reads application connection strings or connects to the application DB.
var counter = new QueryCounter();
using var database = new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>()
    .UseNpgsql($"Host=127.0.0.1;Port=55485;Database=scic_same_schedule_check_{Guid.NewGuid():N};Username=postgres")
    .AddInterceptors(counter).Options, new SyntheticContext());
using var xscribe = new XscribeContext(new DbContextOptions<XscribeContext>());
using var timekeeping = new TimekeepingContext(new DbContextOptions<TimekeepingContext>());
var repository = new EmployeeShiftRepository(database, xscribe, timekeeping);
var service = new EmployeeShiftService(repository);
var start = new DateTime(2026, 10, 16, 8, 0, 0);
var failures = new List<string>();
var passed = 0;
void Check(bool condition, string message)
{
    if (condition) { passed++; Console.WriteLine($"PASS: {message}"); }
    else { failures.Add(message); Console.WriteLine($"FAIL: {message}"); }
}
void SetTimes(object row, int from, int until)
{
    foreach (var day in new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" })
    {
        row.GetType().GetProperty(day + "ShiftStart")!.SetValue(row, start.Date.AddHours(from));
        row.GetType().GetProperty(day + "ShiftEnd")!.SetValue(row, start.Date.AddHours(until));
    }
}
void Employee(int id, string group = "Individual") => database.Employee.Add(new Employee
{
    EmployeeId = id, EmployeeNo = $"QA{id}", FirstName = "Synthetic", LastName = group,
    Suffix = "", Email = "synthetic@example.invalid", ContactNumber = "", CreatedAt = DateTime.UtcNow
});
EmployeeShift Period(int employeeId, int scheduleId, DateTime? from = null, DateTime? until = null)
{
    var row = new EmployeeShift { EmployeeId = employeeId, ShiftId = scheduleId, EffectiveStartDate = from,
        EffectiveEndDate = until, ShiftDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        CreatedBy = "Synthetic original actor", UpdatedAt = DateTime.UtcNow, UpdatedBy = "Synthetic original editor" };
    SetTimes(row, scheduleId == 1 ? 8 : 22, scheduleId == 1 ? 17 : 7);
    database.EmployeeShift.Add(row);
    return row;
}
async Task<string> Snapshot(int id) => JsonSerializer.Serialize(await database.EmployeeShift.AsNoTracking().Where(row => row.EmployeeId == id)
    .OrderBy(row => row.AssignedShiftId).Select(row => new
    {
        row.AssignedShiftId, row.EmployeeId, row.ShiftId, row.DepartmentId, row.ProjectId, row.ShiftDate,
        row.EffectiveStartDate, row.EffectiveEndDate, row.IsTemporary, row.IsFlexibleShift, row.IsNoShift, row.IsNoBreak,
        row.MondayShiftStart, row.MondayShiftEnd, row.TuesdayShiftStart, row.TuesdayShiftEnd,
        row.WednesdayShiftStart, row.WednesdayShiftEnd, row.ThursdayShiftStart, row.ThursdayShiftEnd,
        row.FridayShiftStart, row.FridayShiftEnd, row.SaturdayShiftStart, row.SaturdayShiftEnd,
        row.SundayShiftStart, row.SundayShiftEnd, row.CreatedAt, row.CreatedBy, row.UpdatedAt, row.UpdatedBy, row.Deleted
    }).ToListAsync());
EmployeeShiftFilteredAssignmentRequest Assign(int id, int scheduleId, DateTime? at = null) => new()
{
    ApplyAssignmentToFilter = false, ScheduleId = scheduleId, EffectiveStartDate = at ?? start,
    Changes = [new() { EmployeeId = id, IsAssigned = true }]
};
await database.Database.EnsureCreatedAsync();
try
{
    var day = new Shift { ShiftId = 1, ShiftName = "Synthetic Day", CreatedAt = DateTime.UtcNow };
    var night = new Shift { ShiftId = 2, ShiftName = "Synthetic Night", CreatedAt = DateTime.UtcNow };
    SetTimes(day, 9, 18); SetTimes(night, 22, 7);
    database.Shift.AddRange(day, night);
    foreach (var id in Enumerable.Range(1, 14).Concat([20, 21])) Employee(id);
    var unchanged = Period(1, 1); unchanged.IsNoShift = true; unchanged.IsNoBreak = true;
    unchanged.MondayShiftStart = unchanged.MondayShiftEnd = null;
    Period(2, 1, start.AddDays(-1), start.AddDays(1)); Period(2, 2, start.AddDays(1));
    Period(3, 2, null, start.AddDays(-2)); Period(3, 1, start.AddDays(-2));
    var temporary = Period(4, 1, start.AddHours(-1), start.AddHours(5)); temporary.IsTemporary = true;
    Period(4, 2, start.AddHours(5));
    Period(5, 1, start); Period(6, 2); Period(7, 1); Period(8, 1); Period(9, 1);
    Period(10, 1); Period(10, 2, start.AddHours(-1));
    Period(11, 1, start); Period(12, 1); Period(14, 1).Deleted = true;
    Period(20, 2); Period(21, 2, null, start.AddHours(2)); Period(21, 1, start.AddHours(2));
    for (var index = 1; index <= 251; index++)
    {
        Employee(1000 + index, "Bulk"); Period(1000 + index, index % 2 == 0 ? 2 : 1);
        Employee(2000 + index, "Rollback");
        Period(2000 + index, 2, null, index == 251 ? start.AddHours(2) : null);
        if (index == 251) Period(2000 + index, 1, start.AddHours(2));
    }
    await database.SaveChangesAsync(); database.ChangeTracker.Clear();

    var before = await Snapshot(1);
    var audits = await database.Set<Audit>().CountAsync();
    var request = Assign(1, 1);
    request.Changes[0].IsFlexibleShift = true; // Matching ID ignores requested flag/time changes.
    var result = await service.AssignFilteredAsync(request, "Synthetic new actor");
    Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 0, "Same schedule succeeds with affected zero.");
    Check(await Snapshot(1) == before && await database.Set<Audit>().CountAsync() == audits,
        "Same schedule retains every stored field, history row and audit entry despite changed definition/flags.");

    before = await Snapshot(4);
    request = Assign(4, 1, start.AddHours(1)); request.IsTemporary = true; request.EffectiveEndDate = start.AddHours(10);
    result = await service.AssignFilteredAsync(request, "Synthetic new actor");
    Check(result.Affected == 0 && result.Status == EmployeeShiftAssignmentStatus.Success && await Snapshot(4) == before,
        "Same temporary schedule keeps its original end and restoration, without creating extra periods.");
    before = await Snapshot(5);
    result = await service.AssignFilteredAsync(Assign(5, 1, start), "Synthetic new actor");
    Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 0 && await Snapshot(5) == before,
        "Same schedule at its exact existing start is a no-op rather than a duplicate-transition conflict.");

    before = await Snapshot(2);
    result = await service.AssignFilteredAsync(Assign(2, 2, start.AddDays(1).AddHours(1)), "Synthetic new actor");
    Check(result.Affected == 0 && await Snapshot(2) == before, "Future comparison uses the schedule at the requested effective start.");
    before = await Snapshot(3);
    result = await service.AssignFilteredAsync(Assign(3, 2, start.AddDays(-3)), "Synthetic new actor");
    Check(result.Affected == 0 && await Snapshot(3) == before, "Backdated comparison uses the historical schedule at the requested effective start.");

    result = await service.AssignFilteredAsync(Assign(6, 1), "Synthetic new actor");
    Check(result.Affected == 1 && (await repository.GetAtAsync(6, start))?.ShiftId == 1 &&
        await database.EmployeeShift.CountAsync(row => row.EmployeeId == 6) == 2, "A different schedule still creates audited history.");
    request = Assign(7, 1); request.Changes[0].PreserveSchedule = true; request.Changes[0].IsNoBreak = true;
    result = await service.AssignFilteredAsync(request, "Synthetic new actor");
    Check(result.Affected == 1 && (await repository.GetAtAsync(7, start))!.IsNoBreak,
        "Individual flag-only edits still save even when a matching schedule ID is supplied.");
    result = await service.AssignFilteredAsync(new() { ApplyAssignmentToFilter = false, EffectiveStartDate = start,
        FlagFilters = [new() { SearchKeyword = "QA8", IsNoBreak = true }] }, "Synthetic new actor");
    Check(result.Affected == 1 && (await repository.GetAtAsync(8, start))!.IsNoBreak, "Filtered flag-only edits still create history.");
    result = await service.AssignFilteredAsync(Assign(9, 0), "Synthetic new actor");
    Check(result.Affected == 1 && await repository.GetAtAsync(9, start) == null, "Unassignment still closes the applicable schedule.");

    before = await Snapshot(10);
    result = await service.AssignFilteredAsync(Assign(10, 1), "Synthetic new actor");
    database.ChangeTracker.Clear();
    Check(result.Status == EmployeeShiftAssignmentStatus.Conflict && await Snapshot(10) == before,
        "Ambiguous applicable history still conflicts without mutation.");
    result = await service.AssignFilteredAsync(Assign(11, 2), "Synthetic new actor"); database.ChangeTracker.Clear();
    Check(result.Status == EmployeeShiftAssignmentStatus.Conflict, "Different schedule at an existing transition still conflicts.");
    result = await service.AssignFilteredAsync(Assign(13, 1), "Synthetic new actor");
    Check(result.Affected == 1 && (await repository.GetAtAsync(13, start))?.ShiftId == 1, "An unassigned employee still receives the selected schedule.");
    result = await service.AssignFilteredAsync(Assign(14, 1), "Synthetic new actor");
    Check(result.Affected == 1 && await database.EmployeeShift.CountAsync(row => row.EmployeeId == 14 && !row.Deleted) == 1,
        "Soft-deleted history does not make a new assignment a no-op.");

    before = await Snapshot(1001);
    counter.Count = 0;
    result = await service.AssignFilteredAsync(new() { ScheduleId = 1, EffectiveStartDate = start,
        SearchKeyword = "Bulk", AsOf = start.AddSeconds(-1), Changes =
        [new() { EmployeeId = 1001, IsAssigned = true, IsNoBreak = true }, new() { EmployeeId = 1002, IsAssigned = true }] }, "Synthetic new actor");
    var queries = counter.Count;
    Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 125 && await Snapshot(1001) == before,
        "Mixed 251-employee bulk save skips matching schedules and counts only changed employees, including overrides.");
    Check(queries < 45, "Bulk same-schedule checks use prefetched periods without per-employee queries.");
    audits = await database.Set<Audit>().CountAsync();
    result = await service.AssignFilteredAsync(new() { ScheduleId = 1, EffectiveStartDate = start.AddMinutes(1), SearchKeyword = "Bulk",
        FlagFilters = [new() { SearchKeyword = "Bulk", IsNoBreak = true }] }, "Synthetic new actor");
    Check(result.Status == EmployeeShiftAssignmentStatus.Success && result.Affected == 0 && await database.Set<Audit>().CountAsync() == audits,
        "An entirely matching bulk save returns success with no additional audit entries.");

    var rowCount = await database.EmployeeShift.CountAsync(); audits = await database.Set<Audit>().CountAsync();
    result = await service.AssignFilteredAsync(new() { ScheduleId = 1, EffectiveStartDate = start, IsTemporary = true,
        EffectiveEndDate = start.AddHours(3), SearchKeyword = "Rollback", AsOf = start.AddSeconds(-1) }, "Synthetic new actor");
    database.ChangeTracker.Clear();
    Check(result.Status == EmployeeShiftAssignmentStatus.Conflict && await database.EmployeeShift.CountAsync() == rowCount &&
        await database.Set<Audit>().CountAsync() == audits && (await repository.GetAtAsync(2001, start))?.ShiftId == 2,
        "A later-batch conflict rolls back earlier assignment rows and audit entries.");
}
finally
{
    await database.Database.EnsureDeletedAsync();
}
Console.WriteLine($"{passed} passed; {failures.Count} failed.");
if (failures.Count > 0) Environment.ExitCode = 1;

sealed class SyntheticContext : IHttpContextService
{
    public string GetUser() => "Synthetic actor";
    public string GetReferrer() => "Same-schedule regression checks";
    public string GetSystem() => "Synthetic";
}
sealed class QueryCounter : DbCommandInterceptor
{
    public int Count { get; set; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Count++;
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}

