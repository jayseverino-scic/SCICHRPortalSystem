using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeShiftRepository : Repository, IEmployeeShiftRepository
    {
        public EmployeeShiftRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext) : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var employeeShift = await Context.EmployeeShift!
                        .SingleOrDefaultAsync(s => s.AssignedShiftId == id && !s.Deleted);
            if (employeeShift == null)
                return false;

            employeeShift.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employeeShifts = Context.EmployeeShift!
                .Include(t => t.Employee)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.Shift)
                .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employeeShifts = employeeShifts
                    .Where(e =>
                        e.Employee!.FirstName!.ToLower().Contains(searchKeyword.ToLower()) ||
                        e.Employee.LastName!.ToLower().Contains(searchKeyword.ToLower()));

            }

            var total = employeeShifts.Count();

            employeeShifts = employeeShifts
                .OrderByDescending(e => e.AssignedShiftId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<EmployeeShift>, int>(await employeeShifts.ToListAsync(), total);
        }

        public async Task<IEnumerable<EmployeeShift>> EmployeeShiftFilter(int departmentId, int shiftId)
        {
            IEnumerable<EmployeeShift> employeeShifts;
            if (departmentId != 0 && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false  && e.DepartmentId == departmentId && e.ShiftId == shiftId).ToListAsync();
            }
            else if (departmentId != 0 && shiftId == 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.DepartmentId == departmentId).ToListAsync();
            }
            else if (departmentId == 0 && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include (t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false).ToListAsync();
            }
            return employeeShifts;
        }
        public async Task<IEnumerable<EmployeeShift>> EmployeeShiftFilterPerProject(int projectId, int shiftId)
        {
            IEnumerable<EmployeeShift> employeeShifts;
            if (projectId != 0 && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  //.Include(t => t.Employee)
                  //.Include(t => t.Department)
                  //.Include(t => t.Company)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ProjectId == projectId && e.ShiftId == shiftId).ToListAsync();
            }
            else if (projectId != 0 && shiftId == 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ProjectId == projectId).ToListAsync();
            }
            else if (projectId == 0 && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false).ToListAsync();
            }
            return employeeShifts;
        }
        public async Task<EmployeeShift> GetAsync(int id)
        {
            var employeeShift = await Context.EmployeeShift!
                    .SingleOrDefaultAsync(s => s.AssignedShiftId == id && !s.Deleted);
            return employeeShift!;
        }

        public async Task<IEnumerable<EmployeeShift>> GetAllAsync()
        {
            var employeeShifts = await Context.EmployeeShift!.Where(e => !e.Deleted).ToListAsync();
            return employeeShifts;
        }
        public async Task<DuplicateMessage> HasDuplicateShift(EmployeeShift employeeShift)
        {
            DuplicateMessage message = new();
            var teachers = await Context.EmployeeShift!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicated = teachers.Any(t => t.EmployeeId == employeeShift.EmployeeId);

            if (duplicated)
            {
                message.Message = "Employee Shift Assignment Duplicated";
            }

            message.IsDuplicated = (duplicated);
            return message;
        }

        public async Task InsertAsync(EmployeeShift entity)
        {
            if (entity.IsNoShift)
            {
                entity.IsFlexibleShift = false;
                ClearSchedule(entity);
            }
            await using var transaction = await Context.Database.BeginTransactionAsync();
            await Context.Database.ExecuteSqlRawAsync("""LOCK TABLE "EmployeeShift" IN SHARE ROW EXCLUSIVE MODE""");
            if (await Context.EmployeeShift.AnyAsync(assignment => assignment.EmployeeId == entity.EmployeeId && !assignment.Deleted))
                throw new EmployeeShiftAssignmentConflictException();
            await Context.EmployeeShift!.AddAsync(entity);
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<bool> UpdateAsync(EmployeeShift teacher)
        {
            if (teacher.IsNoShift)
            {
                teacher.IsFlexibleShift = false;
                ClearSchedule(teacher);
            }
            var record = Context.Update(teacher);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRangeAsync(List<EmployeeShift> employeeShifts)
        {
            Context.EmployeeShift!.UpdateRange(employeeShifts);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<EmployeeShift> employeeShifts)
        {
            Context.EmployeeShift!.RemoveRange(employeeShifts);
            await Context.SaveChangesAsync();
        }

        public async Task InsertRangeAsync(List<EmployeeShift> employeeShifts)
        {
            Context.EmployeeShift!.AddRange(employeeShifts);
            await Context.SaveChangesAsync();
        }
        public async Task<EmployeeShift> GetByEmployee(int id)
        {
            var employeeShift = await Context.EmployeeShift!
            .SingleOrDefaultAsync(s => s.EmployeeId == id && !s.Deleted);
            return employeeShift!;
        }

        public async Task<EmployeeShiftFilterPage> GetShiftFilterAsync(int projectId, int shiftId, string filterType, int? skip, int? take, string? searchKeyword, CancellationToken cancellationToken = default)
        {
            var total = await Context.Employee.AsNoTracking().CountAsync(employee => !employee.Deleted, cancellationToken);
            var query = BuildShiftFilterQuery(projectId, shiftId, filterType, searchKeyword);
            var result = await BuildShiftFilterSummaryQuery(query).SingleOrDefaultAsync(cancellationToken) ?? new EmployeeShiftFilterPage();
            query = query.OrderBy(row => row.EmployeeId);

            if (skip.HasValue || take.HasValue)
            {
                query = query.Skip(skip ?? 0).Take(take ?? 10);
            }

            result.Data = await query.ToListAsync(cancellationToken);
            result.Total = total;
            return result;
        }

        private static IQueryable<EmployeeShiftFilterPage> BuildShiftFilterSummaryQuery(IQueryable<EmployeeShiftFilterRow> query)
        {
            // Aggregate the full filter before paging. Empty filters have no group,
            // so the caller returns false flags instead of vacuous all-selected values.
            return query.GroupBy(row => 1).Select(group => new EmployeeShiftFilterPage
            {
                FilteredTotal = group.Count(),
                AllFlexibleShiftSelected = group.Count(row => !row.IsFlexibleShift) == 0,
                AllNoShiftSelected = group.Count(row => !row.IsNoShift) == 0,
                AllNoBreakSelected = group.Count(row => !row.IsNoBreak) == 0
            });
        }

        private IQueryable<EmployeeShiftFilterRow> BuildShiftFilterQuery(int projectId, int shiftId, string filterType, string? searchKeyword)
        {
            // Start from employees so employees without an assignment are also pageable.
            var query = from employee in Context.Employee.AsNoTracking().Where(employee => !employee.Deleted)
                        join assignment in Context.EmployeeShift.AsNoTracking().Where(assignment => !assignment.Deleted)
                            on employee.EmployeeId equals assignment.EmployeeId into assignments
                        from assignment in assignments.DefaultIfEmpty()
                        select new EmployeeShiftFilterRow
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeNo = employee.EmployeeNo,
                            EmployeeName = (employee.LastName ?? "") + ", " + (employee.FirstName ?? ""),
                            DepartmentId = employee.DepartmentId,
                            DepartmentName = employee.Department == null ? null : employee.Department.DepartmentName,
                            ProjectId = employee.ProjectId,
                            ProjectName = employee.Project == null ? null : employee.Project.Name,
                            AssignedShiftId = assignment == null ? 0 : assignment.AssignedShiftId,
                            ShiftId = assignment == null ? 0 : assignment.ShiftId,
                            ShiftName = assignment == null || assignment.Shift == null ? null : assignment.Shift.ShiftName,
                            ShiftDate = assignment == null ? null : assignment.ShiftDate,
                            MondayShiftStart = assignment == null ? null : assignment.MondayShiftStart,
                            MondayShiftEnd = assignment == null ? null : assignment.MondayShiftEnd,
                            TuesdayShiftStart = assignment == null ? null : assignment.TuesdayShiftStart,
                            TuesdayShiftEnd = assignment == null ? null : assignment.TuesdayShiftEnd,
                            WednesdayShiftStart = assignment == null ? null : assignment.WednesdayShiftStart,
                            WednesdayShiftEnd = assignment == null ? null : assignment.WednesdayShiftEnd,
                            ThursdayShiftStart = assignment == null ? null : assignment.ThursdayShiftStart,
                            ThursdayShiftEnd = assignment == null ? null : assignment.ThursdayShiftEnd,
                            FridayShiftStart = assignment == null ? null : assignment.FridayShiftStart,
                            FridayShiftEnd = assignment == null ? null : assignment.FridayShiftEnd,
                            SaturdayShiftStart = assignment == null ? null : assignment.SaturdayShiftStart,
                            SaturdayShiftEnd = assignment == null ? null : assignment.SaturdayShiftEnd,
                            SundayShiftStart = assignment == null ? null : assignment.SundayShiftStart,
                            SundayShiftEnd = assignment == null ? null : assignment.SundayShiftEnd,
                            IsAssigned = assignment != null,
                            IsFlexibleShift = assignment != null && assignment.IsFlexibleShift,
                            IsNoShift = assignment != null && assignment.IsNoShift,
                            IsNoBreak = assignment != null && assignment.IsNoBreak
                        };

            if (projectId > 0) query = query.Where(row => row.ProjectId == projectId);
            if (shiftId > 0) query = query.Where(row => row.ShiftId == shiftId);
            if (filterType == "Assigned") query = query.Where(row => row.IsAssigned);
            if (filterType == "Unassigned") query = query.Where(row => !row.IsAssigned);

            foreach (var word in (searchKeyword ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                // Escape LIKE metacharacters so employee identifiers are searched literally.
                var pattern = "%" + word.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
                query = query.Where(row =>
                    EF.Functions.ILike(row.EmployeeNo ?? "", pattern, "\\") ||
                    EF.Functions.ILike(row.EmployeeName ?? "", pattern, "\\") ||
                    EF.Functions.ILike(row.ProjectName ?? "", pattern, "\\") ||
                    EF.Functions.ILike(row.ShiftName ?? "", pattern, "\\"));
            }

            return query;
        }

        public async Task<bool> UpdateFlagsAsync(int assignedShiftId, EmployeeShiftAssignmentChange change, string actor)
        {
            // Read the assignment after acquiring the same lock as schedule saves,
            // so restoring fixed times cannot use a concurrently replaced schedule.
            await using var transaction = await Context.Database.BeginTransactionAsync();
            await Context.Database.ExecuteSqlRawAsync("""LOCK TABLE "EmployeeShift" IN SHARE ROW EXCLUSIVE MODE""");
            var assignment = await Context.EmployeeShift.SingleOrDefaultAsync(row => row.AssignedShiftId == assignedShiftId &&
                row.EmployeeId == change.EmployeeId && !row.Deleted && row.Employee != null && !row.Employee.Deleted);
            if (assignment == null) return false;
            if (assignment.IsNoShift && !change.IsNoShift)
            {
                var schedule = await Context.Shift.AsNoTracking().SingleOrDefaultAsync(row => row.ShiftId == assignment.ShiftId);
                if (schedule == null) return false;
                CopySchedule(assignment, schedule);
            }
            ApplyFlags(assignment, change);
            assignment.UpdatedBy = actor;
            assignment.UpdatedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<EmployeeShiftAssignmentResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, string actor, CancellationToken cancellationToken = default)
        {
            if (request.ScheduleId < 0 || (request.ApplyAssignmentToFilter && request.ScheduleId == null))
                return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Select a schedule or Unassigned.");
            // Keyset batches keep memory bounded. Serializable isolation keeps one save
            // atomic, including individual overrides, and detects concurrent assignments.
            await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Take the lock before any snapshot reads. Individual inserts use the
                // same lock and recheck assignments after waiting for a bulk save.
                await Context.Database.ExecuteSqlRawAsync("""LOCK TABLE "EmployeeShift" IN SHARE ROW EXCLUSIVE MODE""", cancellationToken);
                Shift? schedule = null;
                if (request.ScheduleId > 0)
                {
                    schedule = await Context.Shift.AsNoTracking().SingleOrDefaultAsync(shift => shift.ShiftId == request.ScheduleId && !shift.Deleted, cancellationToken);
                    if (schedule == null) return new(EmployeeShiftAssignmentStatus.Invalid, Message: "The selected schedule is unavailable.");
                }
                var changes = request.Changes.ToDictionary(change => change.EmployeeId);
                var overrideIds = changes.Keys.ToArray();
                if (await Context.Employee.CountAsync(employee => !employee.Deleted && overrideIds.Contains(employee.EmployeeId), cancellationToken) != overrideIds.Length)
                    return new(EmployeeShiftAssignmentStatus.Invalid, Message: "One or more edited employees are unavailable.");

                var assignmentTargets = BuildShiftFilterQuery(request.ProjectId, request.ShiftId, request.FilterType, request.SearchKeyword)
                    .Where(row => request.ApplyAssignmentToFilter);
                var flagTargets = request.FlagFilters.Select(filter => BuildShiftFilterQuery(filter.ProjectId, filter.ShiftId,
                    filter.FilterType, filter.SearchKeyword)).ToList();
                var targetIds = assignmentTargets.Select(row => row.EmployeeId);
                foreach (var query in flagTargets) targetIds = targetIds.Union(query.Select(row => row.EmployeeId));
                var targets = Context.Employee.AsNoTracking().Where(employee => targetIds.Contains(employee.EmployeeId) && !overrideIds.Contains(employee.EmployeeId))
                    .Select(employee => new AssignmentEmployee { EmployeeId = employee.EmployeeId, DepartmentId = employee.DepartmentId, ProjectId = employee.ProjectId }).Distinct();
                var affected = 0;
                var lastId = 0;
                while (true)
                {
                    var batch = await targets.Where(employee => employee.EmployeeId > lastId).OrderBy(employee => employee.EmployeeId).Take(250).ToListAsync(cancellationToken);
                    if (batch.Count == 0) break;
                    var ids = batch.Select(employee => employee.EmployeeId).ToArray();
                    // Determine all memberships before changing this batch: assigning a
                    // schedule can change the status/shift filters used by flag headers.
                    var assignedIds = (await assignmentTargets.Where(row => ids.Contains(row.EmployeeId))
                        .Select(row => row.EmployeeId).ToListAsync(cancellationToken)).ToHashSet();
                    var flags = new Dictionary<int, EmployeeShiftFilteredFlags>();
                    for (var index = 0; index < flagTargets.Count; index++)
                    {
                        var matches = await flagTargets[index].Where(row => ids.Contains(row.EmployeeId)).Select(row => row.EmployeeId).ToListAsync(cancellationToken);
                        var filter = request.FlagFilters[index];
                        foreach (var id in matches)
                        {
                            if (!flags.TryGetValue(id, out var values)) flags[id] = values = new();
                            if (filter.IsFlexibleShift.HasValue) values.IsFlexibleShift = filter.IsFlexibleShift;
                            if (filter.IsNoShift.HasValue) values.IsNoShift = filter.IsNoShift;
                            if (filter.IsNoBreak.HasValue) values.IsNoBreak = filter.IsNoBreak;
                        }
                    }
                    var result = await SaveAssignmentBatchAsync(batch, schedule, actor, null, cancellationToken, assignedIds, flags);
                    if (result.Status != EmployeeShiftAssignmentStatus.Success) return result;
                    affected += result.Affected;
                    lastId = batch[^1].EmployeeId;
                }
                foreach (var ids in overrideIds.Chunk(250))
                {
                    var batch = await Context.Employee.AsNoTracking().Where(employee => ids.Contains(employee.EmployeeId) && !employee.Deleted)
                        .Select(employee => new AssignmentEmployee { EmployeeId = employee.EmployeeId, DepartmentId = employee.DepartmentId, ProjectId = employee.ProjectId }).ToListAsync(cancellationToken);
                    var result = await SaveAssignmentBatchAsync(batch, schedule, actor, changes, cancellationToken);
                    if (result.Status != EmployeeShiftAssignmentStatus.Success) return result;
                    affected += result.Affected;
                }
                await transaction.CommitAsync(cancellationToken);
                return new(EmployeeShiftAssignmentStatus.Success, affected);
            }
            catch (PostgresException exception) when (exception.SqlState is "40001" or "40P01")
            {
                return AssignmentConflict();
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "40001" or "40P01" })
            {
                return AssignmentConflict();
            }
        }

        private static EmployeeShiftAssignmentResult AssignmentConflict() => new(EmployeeShiftAssignmentStatus.Conflict,
            Message: "Assignments changed while saving. Your selection has been kept. Try again.");

        private async Task<EmployeeShiftAssignmentResult> SaveAssignmentBatchAsync(List<AssignmentEmployee> employees, Shift? schedule, string actor,
            Dictionary<int, EmployeeShiftAssignmentChange>? changes, CancellationToken cancellationToken,
            HashSet<int>? assignmentTargets = null, Dictionary<int, EmployeeShiftFilteredFlags>? filteredFlags = null)
        {
            var ids = employees.Select(employee => employee.EmployeeId).ToArray();
            var assignments = await Context.EmployeeShift.Where(assignment => !assignment.Deleted && ids.Contains(assignment.EmployeeId)).ToListAsync(cancellationToken);
            if (assignments.GroupBy(assignment => assignment.EmployeeId).Any(group => group.Count() > 1))
                return new(EmployeeShiftAssignmentStatus.Conflict, Message: "An employee has multiple active assignments. Resolve that employee before saving.");
            var existing = assignments.ToDictionary(assignment => assignment.EmployeeId);
            var effectiveChanges = changes == null ? new Dictionary<int, EmployeeShiftAssignmentChange>() : new(changes);
            if (assignmentTargets != null)
            {
                foreach (var employee in employees)
                {
                    existing.TryGetValue(employee.EmployeeId, out var assignment);
                    var applyAssignment = assignmentTargets.Contains(employee.EmployeeId);
                    var flags = filteredFlags?.GetValueOrDefault(employee.EmployeeId);
                    if (!applyAssignment && assignment == null && schedule == null)
                        return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Some matching employees have no schedule. Select a schedule or filter Assigned before saving flags.");
                    effectiveChanges[employee.EmployeeId] = new()
                    {
                        EmployeeId = employee.EmployeeId, PreserveSchedule = !applyAssignment && assignment != null,
                        IsAssigned = !applyAssignment || schedule != null,
                        IsFlexibleShift = flags?.IsFlexibleShift ?? assignment?.IsFlexibleShift ?? false,
                        IsNoShift = flags?.IsNoShift ?? (applyAssignment ? false : assignment?.IsNoShift ?? false),
                        IsNoBreak = flags?.IsNoBreak ?? assignment?.IsNoBreak ?? false
                    };
                    if (flags?.IsFlexibleShift == true && flags.IsNoShift != true)
                        effectiveChanges[employee.EmployeeId].IsNoShift = false;
                }
            }
            var restoreIds = assignments.Where(assignment => assignment.IsNoShift &&
                effectiveChanges.GetValueOrDefault(assignment.EmployeeId) is { PreserveSchedule: true, IsNoShift: false })
                .Select(assignment => assignment.ShiftId).Distinct().ToArray();
            var restoreSchedules = restoreIds.Length == 0 ? new Dictionary<int, Shift>() :
                await Context.Shift.AsNoTracking().Where(shift => restoreIds.Contains(shift.ShiftId)).ToDictionaryAsync(shift => shift.ShiftId, cancellationToken);
            var now = DateTime.UtcNow;
            var affected = 0;
            foreach (var employee in employees)
            {
                existing.TryGetValue(employee.EmployeeId, out var assignment);
                var change = effectiveChanges.GetValueOrDefault(employee.EmployeeId);
                if (change?.PreserveSchedule == true)
                {
                    if (assignment == null) return new(EmployeeShiftAssignmentStatus.Invalid, Message: "An edited employee has no active schedule. Select a schedule first.");
                    if (assignment.IsNoShift && !change.IsNoShift)
                    {
                        if (!restoreSchedules.TryGetValue(assignment.ShiftId, out var restored))
                            return new(EmployeeShiftAssignmentStatus.Invalid, Message: "The employee schedule is unavailable.");
                        CopySchedule(assignment, restored);
                    }
                    ApplyFlags(assignment, change);
                    assignment.UpdatedBy = actor;
                    assignment.UpdatedAt = now;
                }
                else if (schedule == null || change?.IsAssigned == false)
                {
                    if (assignment == null) continue;
                    assignment.Deleted = true;
                    assignment.UpdatedAt = now;
                    assignment.UpdatedBy = actor;
                }
                else
                {
                    if (assignment == null)
                    {
                        assignment = new EmployeeShift { EmployeeId = employee.EmployeeId, CreatedAt = now, CreatedBy = actor };
                        Context.EmployeeShift.Add(assignment);
                    }
                    else
                    {
                        assignment.UpdatedAt = now;
                        assignment.UpdatedBy = actor;
                    }
                    assignment.DepartmentId = employee.DepartmentId;
                    assignment.ProjectId = employee.ProjectId;
                    assignment.ShiftId = schedule.ShiftId;
                    assignment.ShiftDate = DateTime.Now;
                    assignment.IsNoShift = false;
                    if (change != null)
                    {
                        ApplyFlags(assignment, change);
                    }
                    CopySchedule(assignment, schedule);
                    if (assignment.IsNoShift) ClearSchedule(assignment);
                }
                affected++;
            }
            cancellationToken.ThrowIfCancellationRequested();
            // The parameterless context overload runs SCIC's existing audit pipeline.
            await Context.SaveChangesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Context.ChangeTracker.Clear();
            return new(EmployeeShiftAssignmentStatus.Success, affected);
        }

        private static void ApplyFlags(EmployeeShift assignment, EmployeeShiftAssignmentChange change)
        {
            assignment.IsNoShift = change.IsNoShift;
            assignment.IsFlexibleShift = !change.IsNoShift && change.IsFlexibleShift;
            assignment.IsNoBreak = change.IsNoBreak;
            if (assignment.IsNoShift) ClearSchedule(assignment);
        }

        private static void ClearSchedule(EmployeeShift assignment)
        {
            assignment.MondayShiftStart = assignment.MondayShiftEnd = null;
            assignment.TuesdayShiftStart = assignment.TuesdayShiftEnd = null;
            assignment.WednesdayShiftStart = assignment.WednesdayShiftEnd = null;
            assignment.ThursdayShiftStart = assignment.ThursdayShiftEnd = null;
            assignment.FridayShiftStart = assignment.FridayShiftEnd = null;
            assignment.SaturdayShiftStart = assignment.SaturdayShiftEnd = null;
            assignment.SundayShiftStart = assignment.SundayShiftEnd = null;
        }

        private static void CopySchedule(EmployeeShift assignment, Shift schedule)
        {
            assignment.MondayShiftStart = schedule.MondayShiftStart;
            assignment.MondayShiftEnd = schedule.MondayShiftEnd;
            assignment.TuesdayShiftStart = schedule.TuesdayShiftStart;
            assignment.TuesdayShiftEnd = schedule.TuesdayShiftEnd;
            assignment.WednesdayShiftStart = schedule.WednesdayShiftStart;
            assignment.WednesdayShiftEnd = schedule.WednesdayShiftEnd;
            assignment.ThursdayShiftStart = schedule.ThursdayShiftStart;
            assignment.ThursdayShiftEnd = schedule.ThursdayShiftEnd;
            assignment.FridayShiftStart = schedule.FridayShiftStart;
            assignment.FridayShiftEnd = schedule.FridayShiftEnd;
            assignment.SaturdayShiftStart = schedule.SaturdayShiftStart;
            assignment.SaturdayShiftEnd = schedule.SaturdayShiftEnd;
            assignment.SundayShiftStart = schedule.SundayShiftStart;
            assignment.SundayShiftEnd = schedule.SundayShiftEnd;
        }

        private sealed class AssignmentEmployee
        {
            public int EmployeeId { get; set; }
            public int? DepartmentId { get; set; }
            public int? ProjectId { get; set; }
        }
    }
}
