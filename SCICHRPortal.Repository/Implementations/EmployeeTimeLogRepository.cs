using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeTimeLogRepository : Repository, IEmployeeTimeLogRepository
    {
        public EmployeeTimeLogRepository(ApplicationContext context)
    : base(context)
        {
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

        public async Task<Tuple<IEnumerable<EmployeeTimeLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate)
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
        public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByDeptAsync(int departmentId, DateTime logDate)
        {
            IEnumerable<EmployeeTimeLog> employeeTimeLogs;
            if (departmentId != 0)
            {
                employeeTimeLogs = await Context.EmployeeTimeLog!
                .Include(t => t.Employee)
                    .ThenInclude(e => e!.Department)
                .Where(e => e.Deleted == false && e.Employee!.DepartmentId == departmentId && e.DateIn == logDate).AsNoTracking()
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
            DuplicateMessage message = new();
            var employeeTimeLogs = await Context.EmployeeTimeLog!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicated = employeeTimeLogs.Any(t => t.EmployeeId == employeeTimeLog.EmployeeId
                        && t.TimeIn == employeeTimeLog.TimeIn && t.TimeOut == employeeTimeLog.TimeOut);

            if (duplicated)
            {
                message.Message = "EmployeeTimeLog Duplicated";
            }

            message.IsDuplicated = duplicated;
            return message;
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
    }
}
