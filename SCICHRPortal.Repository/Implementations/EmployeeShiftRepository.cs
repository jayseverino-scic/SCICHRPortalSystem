using Microsoft.EntityFrameworkCore;
using Npgsql;
using SCICHRPortal.Utility.Helpers;
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
        public async Task<bool> DeleteAsync(int id, string? actor = null)
        {
            var current = await AssignmentsAt(PhilippineTime.Now).AsNoTracking().SingleOrDefaultAsync(row => row.AssignedShiftId == id);
            if (current == null) return false;
            var result = await AssignFilteredAsync(new EmployeeShiftFilteredAssignmentRequest
            {
                EffectiveStartDate = PhilippineTime.Now, ApplyAssignmentToFilter = false, ScheduleId = 0,
                Changes = [new() { EmployeeId = current.EmployeeId, IsAssigned = false }]
            }, actor ?? "Authenticated user");
            return result.Status == EmployeeShiftAssignmentStatus.Success;
        }

        public async Task<Tuple<IEnumerable<EmployeeShift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employeeShifts = AssignmentsAt(PhilippineTime.Now)
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
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false  && e.DepartmentId == departmentId && e.ShiftId == shiftId).ToListAsync();
            }
            else if (departmentId != 0 && shiftId == 0)
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.DepartmentId == departmentId).ToListAsync();
            }
            else if (departmentId == 0 && shiftId != 0)
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include (t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
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
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  //.Include(t => t.Employee)
                  //.Include(t => t.Department)
                  //.Include(t => t.Company)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ProjectId == projectId && e.ShiftId == shiftId).ToListAsync();
            }
            else if (projectId != 0 && shiftId == 0)
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ProjectId == projectId).ToListAsync();
            }
            else if (projectId == 0 && shiftId != 0)
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Project)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await AssignmentsAt(PhilippineTime.Now)
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
            var employeeShifts = await AssignmentsAt(PhilippineTime.Now).AsNoTracking().ToListAsync();
            return employeeShifts;
        }
        public async Task<DuplicateMessage> HasDuplicateShift(EmployeeShift employeeShift)
        {
            DuplicateMessage message = new();
            var teachers = await AssignmentsAt(employeeShift.EffectiveStartDate ?? PhilippineTime.Now)
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
            var result = await WriteEntityAsync(entity);
            if (result.Status != EmployeeShiftAssignmentStatus.Success) throw new EmployeeShiftAssignmentConflictException();
        }

        private Task<EmployeeShiftAssignmentResult> WriteEntityAsync(EmployeeShift entity, DateTime? defaultStart = null) => AssignFilteredAsync(new EmployeeShiftFilteredAssignmentRequest
        {
            EffectiveStartDate = entity.EffectiveStartDate ?? defaultStart ?? PhilippineTime.Now,
            EffectiveEndDate = entity.EffectiveEndDate, IsTemporary = entity.IsTemporary,
            ScheduleId = entity.ShiftId, ApplyAssignmentToFilter = false,
            Changes = [new() { EmployeeId = entity.EmployeeId, IsAssigned = true, IsFlexibleShift = entity.IsFlexibleShift,
                IsNoShift = entity.IsNoShift, IsNoBreak = entity.IsNoBreak }]
        }, entity.UpdatedBy ?? entity.CreatedBy ?? "Authenticated user");

        public async Task<bool> UpdateAsync(EmployeeShift entity)
        {
            if (!await Context.EmployeeShift.AnyAsync(row => row.AssignedShiftId == entity.AssignedShiftId && row.EmployeeId == entity.EmployeeId && !row.Deleted)) return false;
            var result = await WriteEntityAsync(entity);
            if (result.Status == EmployeeShiftAssignmentStatus.Conflict) throw new EmployeeShiftAssignmentConflictException();
            return result.Status == EmployeeShiftAssignmentStatus.Success;
        }

        public async Task UpdateRangeAsync(List<EmployeeShift> employeeShifts)
        {
            await WriteRangeAsync(employeeShifts, true);
        }

        public async Task RemoveRangeAsync(List<EmployeeShift> employeeShifts)
        {
            var result = await AssignFilteredAsync(new EmployeeShiftFilteredAssignmentRequest
            {
                EffectiveStartDate = PhilippineTime.Now, ApplyAssignmentToFilter = false, ScheduleId = 0,
                Changes = employeeShifts.Select(row => new EmployeeShiftAssignmentChange { EmployeeId = row.EmployeeId }).ToList()
            }, employeeShifts.FirstOrDefault()?.UpdatedBy ?? "Authenticated user");
            if (result.Status != EmployeeShiftAssignmentStatus.Success) throw new EmployeeShiftAssignmentConflictException();
        }

        public async Task InsertRangeAsync(List<EmployeeShift> employeeShifts)
        {
            await WriteRangeAsync(employeeShifts, false);
        }

        private async Task WriteRangeAsync(List<EmployeeShift> employeeShifts, bool updating)
        {
            if (employeeShifts.Count == 0) return;
            await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await Context.Database.ExecuteSqlRawAsync("""LOCK TABLE "EmployeeShift" IN SHARE ROW EXCLUSIVE MODE""");
            var start = PhilippineTime.Now;
            foreach (var row in employeeShifts)
            {
                if (updating && !await Context.EmployeeShift.AnyAsync(existing => existing.AssignedShiftId == row.AssignedShiftId &&
                    existing.EmployeeId == row.EmployeeId && !existing.Deleted)) throw new EmployeeShiftAssignmentConflictException();
                var result = await WriteEntityAsync(row, start);
                if (result.Status != EmployeeShiftAssignmentStatus.Success) throw new EmployeeShiftAssignmentConflictException();
            }
            await transaction.CommitAsync();
        }

        public Task<EmployeeShift?> GetByEmployee(int id) => GetAtAsync(id, PhilippineTime.Now);

        public Task<EmployeeShift?> GetAtAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default) =>
            AssignmentsAt(timestamp).AsNoTracking().SingleOrDefaultAsync(row => row.EmployeeId == employeeId, cancellationToken);

        public Task<List<EmployeeShift>> GetPeriodsAsync(int[] employeeIds, DateTime from, DateTime until, CancellationToken cancellationToken = default) =>
            Context.EmployeeShift.AsNoTracking().Where(row => !row.Deleted && employeeIds.Contains(row.EmployeeId) &&
                (row.EffectiveStartDate == null || row.EffectiveStartDate < until) &&
                (row.EffectiveEndDate == null || from < row.EffectiveEndDate)).ToListAsync(cancellationToken);

        public async Task<EmployeeShiftHistoryPage> GetHistoryAsync(int employeeId, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = Context.EmployeeShift.AsNoTracking().Where(row => row.EmployeeId == employeeId && !row.Deleted);
            var total = await query.CountAsync(cancellationToken);
            var rows = await query.OrderBy(row => row.EffectiveStartDate == null).ThenByDescending(row => row.EffectiveStartDate).ThenByDescending(row => row.AssignedShiftId)
                .Skip(skip).Take(take).Select(row => new EmployeeShiftHistoryRow
                {
                    AssignedShiftId = row.AssignedShiftId, ShiftName = row.Shift == null ? null : row.Shift.ShiftName,
                    EffectiveStartDate = row.EffectiveStartDate, EffectiveEndDate = row.EffectiveEndDate, IsTemporary = row.IsTemporary,
                    IsFlexibleShift = row.IsFlexibleShift, IsNoShift = row.IsNoShift, IsNoBreak = row.IsNoBreak,
                    CreatedAt = row.CreatedAt, CreatedBy = row.CreatedBy
                }).ToListAsync(cancellationToken);
            return new EmployeeShiftHistoryPage { Data = rows, Total = total };
        }

        public async Task<EmployeeShiftFilterPage> GetShiftFilterAsync(int projectId, int shiftId, string filterType, int? skip, int? take, string? searchKeyword, CancellationToken cancellationToken = default, DateTime? asOf = null)
        {
            var timestamp = asOf ?? PhilippineTime.Now;
            var total = await Context.Employee.AsNoTracking().CountAsync(employee => !employee.Deleted, cancellationToken);
            var query = BuildShiftFilterQuery(projectId, shiftId, filterType, searchKeyword, timestamp);
            var result = await BuildShiftFilterSummaryQuery(query).SingleOrDefaultAsync(cancellationToken) ?? new EmployeeShiftFilterPage();
            query = query.OrderBy(row => row.EmployeeId);

            if (skip.HasValue || take.HasValue)
            {
                query = query.Skip(skip ?? 0).Take(take ?? 10);
            }

            result.Data = await query.ToListAsync(cancellationToken);
            result.Total = total;
            result.AsOf = timestamp;
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

        private IQueryable<EmployeeShiftFilterRow> BuildShiftFilterQuery(int projectId, int shiftId, string filterType, string? searchKeyword, DateTime? asOf = null)
        {
            // Start from employees so employees without an assignment are also pageable.
            var query = from employee in Context.Employee.AsNoTracking().Where(employee => !employee.Deleted)
                        join assignment in AssignmentsAt(asOf ?? PhilippineTime.Now).AsNoTracking()
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
                            EffectiveStartDate = assignment == null ? null : assignment.EffectiveStartDate,
                            EffectiveEndDate = assignment == null ? null : assignment.EffectiveEndDate,
                            IsTemporary = assignment != null && assignment.IsTemporary,
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
            if (!await Context.EmployeeShift.AnyAsync(row => row.AssignedShiftId == assignedShiftId && row.EmployeeId == change.EmployeeId && !row.Deleted)) return false;
            change.PreserveSchedule = true;
            var result = await AssignFilteredAsync(new EmployeeShiftFilteredAssignmentRequest
            {
                EffectiveStartDate = PhilippineTime.Now, ApplyAssignmentToFilter = false, Changes = [change]
            }, actor);
            if (result.Status == EmployeeShiftAssignmentStatus.Conflict) throw new EmployeeShiftAssignmentConflictException();
            return result.Status == EmployeeShiftAssignmentStatus.Success;
        }

        public async Task<EmployeeShiftAssignmentResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, string actor, CancellationToken cancellationToken = default)
        {
            if (!request.EffectiveStartDate.HasValue || !PhilippineTime.IsLocal(request.EffectiveStartDate) ||
                !PhilippineTime.IsLocal(request.EffectiveEndDate) || !PhilippineTime.IsLocal(request.AsOf) ||
                (request.IsTemporary && (!request.EffectiveEndDate.HasValue || request.EffectiveEndDate <= request.EffectiveStartDate)) ||
                (!request.IsTemporary && request.EffectiveEndDate.HasValue) || request.ScheduleId < 0 || (request.ApplyAssignmentToFilter && request.ScheduleId == null))
                return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Select a schedule or Unassigned.");
            // Keyset batches keep memory bounded. Serializable isolation keeps one save
            // atomic, including individual overrides, and detects concurrent assignments.
            // Legacy range writes own the outer transaction; all entry points share
            // the same lock and audited period writer without partial range saves.
            await using var transaction = Context.Database.CurrentTransaction == null
                ? await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) : null;
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

                var assignmentTargets = BuildShiftFilterQuery(request.ProjectId, request.ShiftId, request.FilterType, request.SearchKeyword, request.AsOf)
                    .Where(row => request.ApplyAssignmentToFilter);
                var flagTargets = request.FlagFilters.Select(filter => BuildShiftFilterQuery(filter.ProjectId, filter.ShiftId,
                    filter.FilterType, filter.SearchKeyword, filter.AsOf ?? request.AsOf)).ToList();
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
                    var result = await SaveAssignmentBatchAsync(batch, schedule, actor, null, request, cancellationToken, assignedIds, flags);
                    if (result.Status != EmployeeShiftAssignmentStatus.Success) return result;
                    affected += result.Affected;
                    lastId = batch[^1].EmployeeId;
                }
                foreach (var ids in overrideIds.Chunk(250))
                {
                    var batch = await Context.Employee.AsNoTracking().Where(employee => ids.Contains(employee.EmployeeId) && !employee.Deleted)
                        .Select(employee => new AssignmentEmployee { EmployeeId = employee.EmployeeId, DepartmentId = employee.DepartmentId, ProjectId = employee.ProjectId }).ToListAsync(cancellationToken);
                    var result = await SaveAssignmentBatchAsync(batch, schedule, actor, changes, request, cancellationToken);
                    if (result.Status != EmployeeShiftAssignmentStatus.Success) return result;
                    affected += result.Affected;
                }
                if (transaction != null) await transaction.CommitAsync(cancellationToken);
                return new(EmployeeShiftAssignmentStatus.Success, affected);
            }
            catch (PostgresException exception) when (exception.SqlState is "40001" or "40P01" or "23505")
            {
                return AssignmentConflict();
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "40001" or "40P01" or "23505" })
            {
                return AssignmentConflict();
            }
        }

        private static EmployeeShiftAssignmentResult AssignmentConflict() => new(EmployeeShiftAssignmentStatus.Conflict,
            Message: "Assignments changed while saving. Your selection has been kept. Try again.");

        private async Task<EmployeeShiftAssignmentResult> SaveAssignmentBatchAsync(List<AssignmentEmployee> employees, Shift? schedule, string actor,
            Dictionary<int, EmployeeShiftAssignmentChange>? changes, EmployeeShiftFilteredAssignmentRequest request, CancellationToken cancellationToken,
            HashSet<int>? assignmentTargets = null, Dictionary<int, EmployeeShiftFilteredFlags>? filteredFlags = null)
        {
            var ids = employees.Select(employee => employee.EmployeeId).ToArray();
            var periods = await Context.EmployeeShift.Where(assignment => !assignment.Deleted && ids.Contains(assignment.EmployeeId)).ToListAsync(cancellationToken);
            var clearedScheduleIds = periods.Where(row => row.IsNoShift).Select(row => row.ShiftId).Distinct().ToArray();
            var fixedSchedules = await Context.Shift.AsNoTracking().Where(row => !row.Deleted && clearedScheduleIds.Contains(row.ShiftId))
                .ToDictionaryAsync(row => row.ShiftId, cancellationToken);
            var start = request.EffectiveStartDate!.Value;
            var affected = 0;
            foreach (var employee in employees)
            {
                var timeline = periods.Where(row => row.EmployeeId == employee.EmployeeId).ToList();
                var applicable = timeline.Where(row => (!row.EffectiveStartDate.HasValue || row.EffectiveStartDate <= start) &&
                    (!row.EffectiveEndDate.HasValue || start < row.EffectiveEndDate)).ToList();
                if (applicable.Count > 1) return AssignmentConflict();
                var previous = applicable.SingleOrDefault();
                var change = changes?.GetValueOrDefault(employee.EmployeeId);
                if (assignmentTargets != null)
                {
                    var applyAssignment = assignmentTargets.Contains(employee.EmployeeId);
                    var flags = filteredFlags?.GetValueOrDefault(employee.EmployeeId);
                    if (!applyAssignment && previous == null && schedule == null)
                        return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Some matching employees have no schedule at the effective start. Select a schedule before saving flags.");
                    change = new()
                    {
                        EmployeeId = employee.EmployeeId, PreserveSchedule = !applyAssignment && previous != null,
                        IsAssigned = !applyAssignment || schedule != null,
                        IsFlexibleShift = flags?.IsFlexibleShift ?? previous?.IsFlexibleShift ?? false,
                        IsNoShift = flags?.IsNoShift ?? (applyAssignment ? false : previous?.IsNoShift ?? false),
                        IsNoBreak = flags?.IsNoBreak ?? previous?.IsNoBreak ?? false
                    };
                    if (flags?.IsFlexibleShift == true && flags.IsNoShift != true) change.IsNoShift = false;
                }
                EmployeeShift? replacement = null;
                if (change?.PreserveSchedule == true)
                {
                    if (previous == null) return new(EmployeeShiftAssignmentStatus.Invalid, Message: "An edited employee has no schedule at the effective start.");
                    replacement = CopyAssignment(previous);
                    if (previous.IsNoShift && !change.IsNoShift)
                    {
                        if (!fixedSchedules.TryGetValue(previous.ShiftId, out var fixedSchedule))
                            return new(EmployeeShiftAssignmentStatus.Invalid, Message: "The employee schedule is unavailable.");
                        CopySchedule(replacement, fixedSchedule);
                    }
                    ApplyFlags(replacement, change);
                }
                else if (schedule != null && change?.IsAssigned != false)
                {
                    replacement = new EmployeeShift { EmployeeId = employee.EmployeeId, DepartmentId = employee.DepartmentId,
                        ProjectId = employee.ProjectId, ShiftId = schedule.ShiftId, ShiftDate = PhilippineTime.Now };
                    CopySchedule(replacement, schedule);
                    if (change != null) ApplyFlags(replacement, change);
                }
                var originalCount = timeline.Count;
                var result = ApplyEffectivePeriod(timeline, replacement, start, request.EffectiveEndDate, request.IsTemporary, actor);
                if (result.Status != EmployeeShiftAssignmentStatus.Success) return result;
                Context.EmployeeShift.AddRange(timeline.Skip(originalCount));
                affected += result.Affected;
            }
            cancellationToken.ThrowIfCancellationRequested();
            await Context.SaveChangesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            Context.ChangeTracker.Clear();
            return new(EmployeeShiftAssignmentStatus.Success, affected);
        }

        private IQueryable<EmployeeShift> AssignmentsAt(DateTime timestamp) => Context.EmployeeShift.Where(row => !row.Deleted &&
            (row.EffectiveStartDate == null || row.EffectiveStartDate <= timestamp) &&
            (row.EffectiveEndDate == null || timestamp < row.EffectiveEndDate));

        private static EmployeeShiftAssignmentResult ApplyEffectivePeriod(List<EmployeeShift> timeline, EmployeeShift? replacement,
            DateTime start, DateTime? end, bool temporary, string actor)
        {
            if (!PhilippineTime.IsLocal(start) || !PhilippineTime.IsLocal(end) || (temporary && (!end.HasValue || end <= start)) || (!temporary && end.HasValue))
                return new(EmployeeShiftAssignmentStatus.Invalid, Message: "Enter a local effective start and a temporary end later than the start.");
            var ordered = timeline.OrderBy(row => row.EffectiveStartDate ?? DateTime.MinValue).ToList();
            for (var index = 1; index < ordered.Count; index++)
            {
                if (ordered[index].EffectiveStartDate == null || ordered[index - 1].EffectiveEndDate == null ||
                    ordered[index - 1].EffectiveEndDate > ordered[index].EffectiveStartDate)
                    return new(EmployeeShiftAssignmentStatus.Conflict, Message: "Overlapping assignment history must be resolved before saving.");
            }
            if (ordered.Any(row => row.EffectiveStartDate == start || row.EffectiveEndDate == start))
                return new(EmployeeShiftAssignmentStatus.Conflict, Message: "An assignment transition already exists at this start time. Choose a distinct time.");
            var previous = ordered.SingleOrDefault(row => (row.EffectiveStartDate == null || row.EffectiveStartDate < start) &&
                (row.EffectiveEndDate == null || start < row.EffectiveEndDate));
            var boundaries = ordered.SelectMany(row => new[] { row.EffectiveStartDate, row.EffectiveEndDate })
                .Where(value => value.HasValue && value > start).Select(value => value!.Value).ToList();
            DateTime? next = boundaries.Count == 0 ? null : boundaries.Min();
            if (temporary && next.HasValue && end > next)
                return new(EmployeeShiftAssignmentStatus.Conflict, Message: "The temporary period crosses a scheduled transition. Choose an earlier end.");
            if (previous == null && replacement == null) return new(EmployeeShiftAssignmentStatus.Success, 0);
            var resume = temporary && previous != null && (!next.HasValue || end < next) ? CopyAssignment(previous) : null;
            var now = DateTime.UtcNow;
            if (previous != null)
            {
                previous.EffectiveEndDate = start;
                previous.UpdatedAt = now;
                previous.UpdatedBy = actor;
            }
            void Add(EmployeeShift row, DateTime from, DateTime? until, bool isTemporary)
            {
                row.AssignedShiftId = 0;
                row.EffectiveStartDate = from;
                row.EffectiveEndDate = until;
                row.IsTemporary = isTemporary;
                row.CreatedAt = now;
                row.CreatedBy = actor;
                row.UpdatedAt = null;
                row.UpdatedBy = null;
                row.Deleted = false;
                timeline.Add(row);
            }
            if (replacement != null) Add(replacement, start, temporary ? end : next, temporary);
            if (resume != null) Add(resume, end!.Value, next, false);
            return new(EmployeeShiftAssignmentStatus.Success, 1);
        }

        private static EmployeeShift CopyAssignment(EmployeeShift row) => new()
        {
            EmployeeId = row.EmployeeId, DepartmentId = row.DepartmentId, ProjectId = row.ProjectId, ShiftId = row.ShiftId,
            ShiftDate = row.ShiftDate, IsFlexibleShift = row.IsFlexibleShift, IsNoShift = row.IsNoShift, IsNoBreak = row.IsNoBreak,
            MondayShiftStart = row.MondayShiftStart, MondayShiftEnd = row.MondayShiftEnd,
            TuesdayShiftStart = row.TuesdayShiftStart, TuesdayShiftEnd = row.TuesdayShiftEnd,
            WednesdayShiftStart = row.WednesdayShiftStart, WednesdayShiftEnd = row.WednesdayShiftEnd,
            ThursdayShiftStart = row.ThursdayShiftStart, ThursdayShiftEnd = row.ThursdayShiftEnd,
            FridayShiftStart = row.FridayShiftStart, FridayShiftEnd = row.FridayShiftEnd,
            SaturdayShiftStart = row.SaturdayShiftStart, SaturdayShiftEnd = row.SaturdayShiftEnd,
            SundayShiftStart = row.SundayShiftStart, SundayShiftEnd = row.SundayShiftEnd
        };

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
