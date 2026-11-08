using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class TardinessRepository : Repository, ITardinessRepository
    {
        public TardinessRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
            : base(context, xscribeContext, timekeepingContext) { }

        public async Task<TardinessPage> GetPageAsync(TardinessPageQuery query, CancellationToken cancellationToken = default)
        {
            if (!query.IsValid()) throw new ArgumentException("Invalid tardiness page query.", nameof(query));
            var source = ActiveAttendance();
            var total = await source.CountAsync(cancellationToken);
            var filtered = Filter(source, query);
            var count = await filtered.CountAsync(cancellationToken);
            var rows = await Order(Project(filtered), query).Skip(query.Start).Take(query.Length).ToListAsync(cancellationToken);
            return new TardinessPage { Draw = query.Draw, RecordsTotal = total, RecordsFiltered = count, Data = rows };
        }

        public async Task<List<TardinessRow>> GetExportAsync(TardinessFilterQuery query, CancellationToken cancellationToken = default)
        {
            if (!query.IsValid()) throw new ArgumentException("Invalid tardiness export query.", nameof(query));
            return await Order(Project(Filter(ActiveAttendance(), query)), query).ToListAsync(cancellationToken);
        }

        private IQueryable<EmployeeAttendance> ActiveAttendance() => Context.EmployeeAttendance.AsNoTracking()
            .Where(row => !row.Deleted && row.Employee != null && !row.Employee.Deleted &&
                row.EmployeeTimeLog != null && !row.EmployeeTimeLog.Deleted);

        private static IQueryable<EmployeeAttendance> Filter(IQueryable<EmployeeAttendance> source, TardinessFilterQuery query)
        {
            // AttendanceDate follows existing attendance reports: the saved TimeIn's business date.
            if (query.FromDate is { } fromDate)
            {
                var from = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Unspecified);
                source = source.Where(row => row.TimeIn >= from);
            }
            if (query.ToDate is { } toDate)
            {
                var until = DateTime.SpecifyKind(toDate.Date.AddDays(1), DateTimeKind.Unspecified);
                source = source.Where(row => row.TimeIn < until);
            }
            if (query.ProjectId > 0) source = source.Where(row => row.Employee!.ProjectId == query.ProjectId);
            foreach (var word in (query.Search ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var term = word.ToUpperInvariant();
                source = source.Where(row => (row.Employee!.EmployeeNo ?? "").ToUpper().Contains(term) ||
                    ((row.Employee.LastName ?? "") + ", " + (row.Employee.FirstName ?? "")).ToUpper().Contains(term));
            }
            return source;
        }

        private static IQueryable<TardinessRow> Project(IQueryable<EmployeeAttendance> source) => source.Select(row => new TardinessRow
        {
            EmployeeAttendanceId = row.EmployeeAttendanceId,
            EmployeeNo = row.Employee!.EmployeeNo,
            EmployeeName = (row.Employee.LastName ?? "") + ", " + (row.Employee.FirstName ?? ""),
            DepartmentName = row.Employee.Department == null || row.Employee.Department.Deleted ? null : row.Employee.Department.DepartmentName,
            ProjectName = row.Employee.Project == null || row.Employee.Project.Deleted ? null : row.Employee.Project.Name,
            AttendanceDate = row.TimeIn.Date, ShiftStart = row.ShiftStart, ShiftEnd = row.ShiftEnd,
            TimeIn = row.TimeIn, TimeOut = row.TimeOut, LateMinutes = row.ShiftLate, UndertimeMinutes = row.ShiftUndertime
        });

        private static IOrderedQueryable<TardinessRow> Order(IQueryable<TardinessRow> source, TardinessFilterQuery query)
        {
            IOrderedQueryable<TardinessRow>? ordered = null;
            for (var index = 0; index < query.SortColumns.Length; index++)
            {
                var key = SortKey(query.SortColumns[index]);
                var descending = query.SortDirections[index] == "desc";
                ordered = ordered is null
                    ? descending ? source.OrderByDescending(key) : source.OrderBy(key)
                    : descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
            }
            return (ordered ?? source.OrderByDescending(row => row.TimeIn)).ThenBy(row => row.EmployeeAttendanceId);
        }

        private static Expression<Func<TardinessRow, object?>> SortKey(string column) => column switch
        {
            "employeeNo" => row => row.EmployeeNo, "employeeName" => row => row.EmployeeName,
            "departmentName" => row => row.DepartmentName, "projectName" => row => row.ProjectName,
            "attendanceDate" => row => row.AttendanceDate, "shiftStart" => row => row.ShiftStart,
            "shiftEnd" => row => row.ShiftEnd, "timeIn" => row => row.TimeIn, "timeOut" => row => row.TimeOut,
            "lateMinutes" => row => row.LateMinutes, "undertimeMinutes" => row => row.UndertimeMinutes,
            _ => throw new ArgumentException("Unsupported tardiness sort column.", nameof(column))
        };
    }
}
