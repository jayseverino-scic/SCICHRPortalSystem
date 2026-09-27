using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public partial class EmployeeTimeLogRepository : Repository, IEmployeeTimeLogRepository
    {
        public EmployeeTimeLogRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<EmployeeTimeLogEmployeeLookupPage> SearchEmployeesAsync(string term, int page)
        {
            const int pageSize = 20;
            var normalized = term.Trim().ToUpper();
            var rows = await Context.Employee!
                .AsNoTracking()
                .Where(e => !e.Deleted &&
                    ((e.EmployeeNo ?? "").ToUpper().Contains(normalized) ||
                     (e.FirstName ?? "").ToUpper().Contains(normalized) ||
                     (e.LastName ?? "").ToUpper().Contains(normalized) ||
                     ((e.FirstName ?? "") + " " + (e.LastName ?? "")).ToUpper().Contains(normalized) ||
                     ((e.LastName ?? "") + " " + (e.FirstName ?? "")).ToUpper().Contains(normalized)))
                .OrderBy(e => e.EmployeeId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize + 1)
                .Select(e => new EmployeeTimeLogEmployeeLookupItem
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeNo = e.EmployeeNo,
                    FirstName = e.FirstName,
                    LastName = e.LastName
                })
                .ToListAsync();
            return new EmployeeTimeLogEmployeeLookupPage
            {
                Data = rows.Take(pageSize).ToList(),
                More = rows.Count > pageSize
            };
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await Context.EmployeeTimeLog!
                        .SingleOrDefaultAsync(s => s.TimeLogId == id && !s.Deleted);
            if (item == null)
                return false;

            item.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<EmployeeTimeLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            var employeeTimeLogs = Context.EmployeeTimeLog!
                .Include(e => e.Employee)
                .Where(e => e.Deleted == false);

            if (startDate.HasValue && endDate.HasValue)
                employeeTimeLogs = employeeTimeLogs.Where(e => e.DateIn >= startDate && e.DateIn <= endDate).AsNoTracking();

            var total = employeeTimeLogs.Count();

            employeeTimeLogs = employeeTimeLogs
                .OrderByDescending(e => e.TimeLogId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<EmployeeTimeLog>, int>(await employeeTimeLogs.ToListAsync(), total);
        }
        //public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByDeptAsync(int departmentId, DateTime logDate)
        //{
        //    IEnumerable<EmployeeTimeLog> employeeTimeLogs;
        //    if (departmentId != 0)
        //    {
        //        employeeTimeLogs = await Context.EmployeeTimeLog!
        //        .Include(t => t.Employee)
        //            .ThenInclude(e => e!.Department)
        //        .Where(e => e.Deleted == false && e.Employee!.Department_Id == departmentId && e.DateIn == logDate).AsNoTracking()
        //        .ToListAsync();
        //    }
        //    else
        //    {
        //        employeeTimeLogs = await Context.EmployeeTimeLog!
        //          .Include(t => t.Employee)
        //          .Where(e => e.Deleted == false && e.DateIn == logDate).AsNoTracking().ToListAsync();
        //    }
        //    return employeeTimeLogs;
        //}
        public async Task<IEnumerable<EmployeeTimeLog>> GetAllAsync()
        {
            var employeeTimeLogs = await Context.EmployeeTimeLog!.Where(s => !s.Deleted)
              .ToListAsync();
            return employeeTimeLogs;
        }

        public async Task<EmployeeTimeLog> GetAsync(int id)
        {
            var item = await Context.EmployeeTimeLog!
                    .SingleOrDefaultAsync(s => s.TimeLogId == id && !s.Deleted);
            return item!;
        }

        public async Task<DuplicateMessage> HasDuplicateName(EmployeeTimeLog employeeTimeLog)
        {
            var dateIn = employeeTimeLog.DateIn?.Date;
            var dateOut = employeeTimeLog.DateOut?.Date;
            var duplicated = await Context.EmployeeTimeLog!.AnyAsync(t =>
                !t.Deleted && t.TimeLogId != employeeTimeLog.TimeLogId &&
                t.EmployeeId == employeeTimeLog.EmployeeId &&
                t.DateIn.HasValue && t.DateIn.Value.Date == dateIn &&
                t.DateOut.HasValue && t.DateOut.Value.Date == dateOut &&
                t.TimeIn == employeeTimeLog.TimeIn && t.TimeOut == employeeTimeLog.TimeOut);

            return new DuplicateMessage
            {
                IsDuplicated = duplicated,
                Message = duplicated ? "A time log with the same employee, dates, and times already exists." : null
            };
        }

        public async Task InsertAsync(EmployeeTimeLog entity)
        {
            await Context.EmployeeTimeLog!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(EmployeeTimeLog employeeTimeLog)
        {
            var record = Context.Update(employeeTimeLog);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<EmployeeTimeLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            var query = Context.EmployeeTimeLog!
                .AsNoTracking()
                .Include(e => e.Employee)
                .Where(e => !e.Deleted);

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(e => e.DateIn >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;
                // An exclusive next-day boundary includes the entire end date.
                if (end < DateTime.MaxValue.Date)
                {
                    var endExclusive = end.AddDays(1);
                    query = query.Where(e => e.DateIn < endExclusive);
                }
                else
                {
                    query = query.Where(e => e.DateIn <= DateTime.MaxValue);
                }
            }
            if (!string.IsNullOrWhiteSpace(projectName))
            {
                var project = projectName.ToUpper();
                query = query.Where(e => e.ProjectTimeIn != null && e.ProjectTimeIn.ToUpper() == project);
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByProjectAsync(int projectId, DateTime logDate)
        {
            IEnumerable<EmployeeTimeLog> employeeTimeLogs;
            if (projectId != 0)
            {
                employeeTimeLogs = await Context.EmployeeTimeLog!
                .Include(t => t.Employee)
                    .ThenInclude(e => e!.Department)
                .Where(e => e.Deleted == false && e.Employee!.ProjectId == projectId && e.DateIn == logDate).AsNoTracking()
                .ToListAsync();
            }
            else
            {
                employeeTimeLogs = await Context.EmployeeTimeLog!
                  .Include(t => t.Employee)
                  .Where(e => e.Deleted == false && e.DateIn == logDate).AsNoTracking().ToListAsync();
            }
            return employeeTimeLogs;
        }
    }
}
