using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class LeaveRepository : Repository, ILeaveRepository
    {
        public LeaveRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
            : base(context, xscribeContext, timekeepingContext)
        {
        }

        private static readonly Expression<Func<Leave, LeaveRow>> Row = record => new LeaveRow
        {
            LeaveId = record.LeaveId,
            EmployeeId = record.EmployeeId,
            EmployeeNo = record.Employee!.EmployeeNo,
            EmployeeName = ((record.Employee.LastName ?? "") + ", " + (record.Employee.FirstName ?? "") +
                " " + (record.Employee.MiddleName ?? "") + " " + (record.Employee.Suffix ?? "")).Trim(),
            EmployeeDeleted = record.Employee.Deleted,
            LeaveTypeId = record.LeaveTypeId,
            LeaveType = record.LeaveType!.LeaveDescription,
            LeaveTypeDeleted = record.LeaveType.Deleted,
            From = record.From,
            To = record.To
        };

        public async Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken)
        {
            var records = Context.Leave.AsNoTracking().Where(record => !record.Deleted).Select(Row);
            var total = await records.CountAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToUpperInvariant();
                records = records.Where(record => (record.EmployeeNo ?? "").ToUpper().Contains(term) ||
                    (record.EmployeeName ?? "").ToUpper().Contains(term) || (record.LeaveType ?? "").ToUpper().Contains(term));
            }
            var filtered = await records.CountAsync(cancellationToken);
            var ascending = query.SortDirection == "asc";
            var ordered = query.SortColumn switch
            {
                "employeeNo" => ascending ? records.OrderBy(record => record.EmployeeNo) : records.OrderByDescending(record => record.EmployeeNo),
                "employeeName" => ascending ? records.OrderBy(record => record.EmployeeName) : records.OrderByDescending(record => record.EmployeeName),
                "to" => ascending ? records.OrderBy(record => record.To) : records.OrderByDescending(record => record.To),
                "leaveType" => ascending ? records.OrderBy(record => record.LeaveType) : records.OrderByDescending(record => record.LeaveType),
                _ => ascending ? records.OrderBy(record => record.From) : records.OrderByDescending(record => record.From)
            };
            return new LeavePage
            {
                Draw = query.Draw, RecordsTotal = total, RecordsFiltered = filtered,
                Data = await ordered.ThenByDescending(record => record.LeaveId)
                    .Skip(query.Start).Take(query.Length).ToListAsync(cancellationToken)
            };
        }

        public Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Context.Leave.AsNoTracking().Where(record => record.LeaveId == id && !record.Deleted)
                .Select(Row).SingleOrDefaultAsync(cancellationToken);

        public async Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken)
        {
            const int pageSize = 20;
            var search = term.Trim().ToUpperInvariant();
            var employees = Context.Employee.AsNoTracking().Where(employee => !employee.Deleted)
                .Select(employee => new LeaveEmployeeLookupItem
                {
                    EmployeeId = employee.EmployeeId, EmployeeNo = employee.EmployeeNo,
                    EmployeeName = ((employee.LastName ?? "") + ", " + (employee.FirstName ?? "") +
                        " " + (employee.MiddleName ?? "") + " " + (employee.Suffix ?? "")).Trim()
                });
            var rows = await employees.Where(employee => (employee.EmployeeNo ?? "").ToUpper().Contains(search) ||
                    (employee.EmployeeName ?? "").ToUpper().Contains(search))
                .OrderBy(employee => employee.EmployeeNo).ThenBy(employee => employee.EmployeeId)
                .Skip((page - 1) * pageSize).Take(pageSize + 1).ToListAsync(cancellationToken);
            return new LeaveEmployeeLookupPage { Data = rows.Take(pageSize).ToList(), More = rows.Count > pageSize };
        }

        public Task<List<LeaveImportEmployee>> GetImportEmployeesAsync(IReadOnlyCollection<string> normalizedEmployeeNumbers, CancellationToken cancellationToken) =>
            Context.Employee.AsNoTracking().Where(employee => !employee.Deleted && employee.EmployeeNo != null &&
                normalizedEmployeeNumbers.Contains(employee.EmployeeNo.Trim().ToUpper()))
                .Select(employee => new LeaveImportEmployee(employee.EmployeeId, employee.EmployeeNo)).ToListAsync(cancellationToken);

        public Task<Leave?> GetAsync(int id, CancellationToken cancellationToken) =>
            Context.Leave.SingleOrDefaultAsync(record => record.LeaveId == id && !record.Deleted, cancellationToken);

        public Task<List<LeaveTypeLookupItem>> GetLeaveTypesAsync(CancellationToken cancellationToken) =>
            Context.LeaveType.AsNoTracking().Where(type => !type.Deleted)
                .OrderBy(type => type.LeaveDescription).ThenBy(type => type.LeaveTypeId)
                .Select(type => new LeaveTypeLookupItem(type.LeaveTypeId, type.LeaveDescription)).ToListAsync(cancellationToken);

        public Task<List<LeaveTypeLookupItem>> GetImportLeaveTypesAsync(IReadOnlyCollection<string> normalizedDescriptions, CancellationToken cancellationToken) =>
            Context.LeaveType.AsNoTracking().Where(type => !type.Deleted && type.LeaveDescription != null &&
                normalizedDescriptions.Contains(type.LeaveDescription.Trim().ToUpper()))
                .Select(type => new LeaveTypeLookupItem(type.LeaveTypeId, type.LeaveDescription)).ToListAsync(cancellationToken);

        public Task<bool> LeaveTypeExistsAsync(int id, CancellationToken cancellationToken) =>
            Context.LeaveType.AnyAsync(type => type.LeaveTypeId == id && !type.Deleted, cancellationToken);

        public Task<bool> EmployeeExistsAsync(int id, CancellationToken cancellationToken) =>
            Context.Employee.AnyAsync(employee => employee.EmployeeId == id && !employee.Deleted, cancellationToken);

        public Task<bool> HasOverlapAsync(int employeeId, DateTime from, DateTime to, int? excludingId, CancellationToken cancellationToken) =>
            Context.Leave.AnyAsync(record => !record.Deleted && record.EmployeeId == employeeId &&
                (!excludingId.HasValue || record.LeaveId != excludingId.Value) && record.From < to && record.To > from, cancellationToken);

        public async Task InsertAsync(Leave record, CancellationToken cancellationToken)
        {
            Context.Leave.Add(record);
            await SaveAsync(cancellationToken);
        }

        public async Task UpdateAsync(Leave record, Leave changes, CancellationToken cancellationToken)
        {
            record.EmployeeId = changes.EmployeeId;
            record.LeaveTypeId = changes.LeaveTypeId;
            record.From = changes.From;
            record.To = changes.To;
            record.UpdatedAt = changes.UpdatedAt;
            record.UpdatedBy = changes.UpdatedBy;

            await SaveAsync(cancellationToken);
        }

        public async Task DeleteAsync(Leave record, Leave changes, CancellationToken cancellationToken)
        {
            record.Deleted = changes.Deleted;
            record.UpdatedAt = changes.UpdatedAt;
            record.UpdatedBy = changes.UpdatedBy;

            await SaveAsync(cancellationToken);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // The parameterless overload includes SCIC's audit entries (including generated keys).
            await Context.SaveChangesAsync();
        }

        public async Task<TResult> ExecuteWriteAsync<TResult>(
            Func<Task<(TResult Result, bool Commit)>> operation, CancellationToken cancellationToken)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // The overlap predicate and audited saves must share this transaction so PostgreSQL
                // detects concurrent inserts/edits even when neither could see the other's row.
                var (result, commit) = await operation();
                if (commit)
                    await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception) when (IsWriteConflict(exception))
            {
                Context.ChangeTracker.Clear();
                throw new LeavePersistenceConflictException(exception);
            }
        }

        private static bool IsWriteConflict(Exception exception)
        {
            for (Exception? current = exception; current != null; current = current.InnerException)
                if (current is PostgresException { SqlState: "40001" or "40P01" }) return true;
            return false;
        }
    }
}
