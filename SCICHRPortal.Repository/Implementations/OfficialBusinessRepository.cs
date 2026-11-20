using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class OfficialBusinessRepository : Repository, IOfficialBusinessRepository
    {
        public OfficialBusinessRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
            : base(context, xscribeContext, timekeepingContext)
        {
        }

        private static readonly Expression<Func<OfficialBusiness, OfficialBusinessRow>> Row = record => new OfficialBusinessRow
        {
            OfficialBusinessId = record.OfficialBusinessId,
            EmployeeId = record.EmployeeId,
            EmployeeNo = record.Employee!.EmployeeNo,
            EmployeeName = ((record.Employee.LastName ?? "") + ", " + (record.Employee.FirstName ?? "") +
                " " + (record.Employee.MiddleName ?? "") + " " + (record.Employee.Suffix ?? "")).Trim(),
            EmployeeDeleted = record.Employee.Deleted,
            From = record.From,
            To = record.To
        };

        public async Task<OfficialBusinessPage> GetPageAsync(OfficialBusinessPageQuery query, CancellationToken cancellationToken)
        {
            var records = Context.OfficialBusiness.AsNoTracking().Where(record => !record.Deleted).Select(Row);
            var total = await records.CountAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToUpperInvariant();
                records = records.Where(record => (record.EmployeeNo ?? "").ToUpper().Contains(term) ||
                    (record.EmployeeName ?? "").ToUpper().Contains(term));
            }
            var filtered = await records.CountAsync(cancellationToken);
            var ascending = query.SortDirection == "asc";
            var ordered = query.SortColumn switch
            {
                "employeeNo" => ascending ? records.OrderBy(record => record.EmployeeNo) : records.OrderByDescending(record => record.EmployeeNo),
                "employeeName" => ascending ? records.OrderBy(record => record.EmployeeName) : records.OrderByDescending(record => record.EmployeeName),
                "to" => ascending ? records.OrderBy(record => record.To) : records.OrderByDescending(record => record.To),
                _ => ascending ? records.OrderBy(record => record.From) : records.OrderByDescending(record => record.From)
            };
            return new OfficialBusinessPage
            {
                Draw = query.Draw, RecordsTotal = total, RecordsFiltered = filtered,
                Data = await ordered.ThenByDescending(record => record.OfficialBusinessId)
                    .Skip(query.Start).Take(query.Length).ToListAsync(cancellationToken)
            };
        }

        public Task<OfficialBusinessRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            Context.OfficialBusiness.AsNoTracking().Where(record => record.OfficialBusinessId == id && !record.Deleted)
                .Select(Row).SingleOrDefaultAsync(cancellationToken);

        public async Task<OfficialBusinessEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken)
        {
            const int pageSize = 20;
            var search = term.Trim().ToUpperInvariant();
            var employees = Context.Employee.AsNoTracking().Where(employee => !employee.Deleted)
                .Select(employee => new OfficialBusinessEmployeeLookupItem
                {
                    EmployeeId = employee.EmployeeId, EmployeeNo = employee.EmployeeNo,
                    EmployeeName = ((employee.LastName ?? "") + ", " + (employee.FirstName ?? "") +
                        " " + (employee.MiddleName ?? "") + " " + (employee.Suffix ?? "")).Trim()
                });
            var rows = await employees.Where(employee => (employee.EmployeeNo ?? "").ToUpper().Contains(search) ||
                    (employee.EmployeeName ?? "").ToUpper().Contains(search))
                .OrderBy(employee => employee.EmployeeNo).ThenBy(employee => employee.EmployeeId)
                .Skip((page - 1) * pageSize).Take(pageSize + 1).ToListAsync(cancellationToken);
            return new OfficialBusinessEmployeeLookupPage { Data = rows.Take(pageSize).ToList(), More = rows.Count > pageSize };
        }

        public Task<List<OfficialBusinessImportEmployee>> GetImportEmployeesAsync(IReadOnlyCollection<string> normalizedEmployeeNumbers, CancellationToken cancellationToken) =>
            Context.Employee.AsNoTracking().Where(employee => !employee.Deleted && employee.EmployeeNo != null &&
                normalizedEmployeeNumbers.Contains(employee.EmployeeNo.Trim().ToUpper()))
                .Select(employee => new OfficialBusinessImportEmployee(employee.EmployeeId, employee.EmployeeNo)).ToListAsync(cancellationToken);

        public Task<OfficialBusiness?> GetAsync(int id, CancellationToken cancellationToken) =>
            Context.OfficialBusiness.SingleOrDefaultAsync(record => record.OfficialBusinessId == id && !record.Deleted, cancellationToken);

        public Task<bool> EmployeeExistsAsync(int id, CancellationToken cancellationToken) =>
            Context.Employee.AnyAsync(employee => employee.EmployeeId == id && !employee.Deleted, cancellationToken);

        public Task<bool> HasOverlapAsync(int employeeId, DateTime from, DateTime to, int? excludingId, CancellationToken cancellationToken) =>
            Context.OfficialBusiness.AnyAsync(record => !record.Deleted && record.EmployeeId == employeeId &&
                (!excludingId.HasValue || record.OfficialBusinessId != excludingId.Value) && record.From < to && record.To > from, cancellationToken);

        public async Task InsertAsync(OfficialBusiness record, CancellationToken cancellationToken)
        {
            Context.OfficialBusiness.Add(record);
            await SaveAsync(cancellationToken);
        }

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // The parameterless overload includes SCIC's audit entries (including generated keys).
            await Context.SaveChangesAsync();
        }

        public async Task<OfficialBusinessWriteResult> ExecuteWriteAsync(Func<Task<OfficialBusinessWriteResult>> operation, CancellationToken cancellationToken)
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                // The overlap predicate and audited saves must share this transaction so PostgreSQL
                // detects concurrent inserts/edits even when neither could see the other's row.
                var result = await operation();
                if (result.Status == OfficialBusinessWriteStatus.Success)
                    await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception) when (IsWriteConflict(exception))
            {
                Context.ChangeTracker.Clear();
                return new(OfficialBusinessWriteStatus.Conflict, "The records changed while saving. Reload and try again.");
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
