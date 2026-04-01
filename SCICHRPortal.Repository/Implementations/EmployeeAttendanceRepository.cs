using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeAttendanceRepository : Repository, IEmployeeAttendanceRepository
    {
        public EmployeeAttendanceRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<Tuple<IEnumerable<EmployeeAttendance>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employeeAttendances = Context.EmployeeAttendance!
                .Include(t => t.Employee)
                .Include(t => t.EmployeeTimeLog)
                .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employeeAttendances = employeeAttendances
                    .Where(e =>
                        e.Employee!.FirstName!.ToLower().Contains(searchKeyword.ToLower()) ||
                        e.Employee.LastName!.ToLower().Contains(searchKeyword.ToLower()));

            }

            var total = employeeAttendances.Count();

            employeeAttendances = employeeAttendances
                .OrderByDescending(e => e.EmployeeAttendanceId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<EmployeeAttendance>, int>(await employeeAttendances.ToListAsync(), total);
        }

        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceFilter(int departmentId, DateTime attendanceDate)
        {
            IEnumerable<EmployeeAttendance> employeeAttendances;
            if (departmentId != 0)
            {
                employeeAttendances = await Context.EmployeeAttendance!
                  .Include(t => t.Employee)
                  .Include(t => t.EmployeeTimeLog)
                  .Where(e => e.Deleted == false && e.Employee!.DepartmentId == departmentId && e.TimeIn.Date == attendanceDate.Date).ToListAsync();
            }
            else
            {
                employeeAttendances = await Context.EmployeeAttendance!
                  .Include(t => t.Employee)
                  .Include(t => t.EmployeeTimeLog)
                  .Where(e => e.Deleted == false && e.TimeIn.Date == attendanceDate.Date).ToListAsync() ;
            }
            return employeeAttendances;
        }


        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceCutOffFilter(int departmentId, DateTime fromDate, DateTime toDate)
        {
            IEnumerable<EmployeeAttendance> employeeAttendances;
            if (departmentId != 0)
            {
                employeeAttendances = await Context.EmployeeAttendance!
                  .Include(t => t.Employee)
                  .Include(t => t.EmployeeTimeLog)
                  .Where(e => e.Deleted == false && e.Employee!.DepartmentId == departmentId && e.TimeIn.Date >= fromDate.Date && e.TimeIn.Date <= toDate.Date).ToListAsync();
            }
            else
            {
                employeeAttendances = await Context.EmployeeAttendance!
                  .Include(t => t.Employee)
                  .Include(t => t.EmployeeTimeLog)
                  .Where(e => e.Deleted == false && e.TimeIn.Date >= fromDate.Date && e.TimeIn.Date <= toDate.Date).ToListAsync();
            }
            return employeeAttendances;
        }
        public async Task<EmployeeAttendance> GetAsync(int id)
        {
            var employeeAttendance = await Context.EmployeeAttendance!
                    .SingleOrDefaultAsync(s => s.EmployeeAttendanceId == id && !s.Deleted);
            return employeeAttendance!;
        }

        public async Task<IEnumerable<EmployeeAttendance>> GetAllAsync()
        {
            var employeeAttendances = await Context.EmployeeAttendance!.Where(e => !e.Deleted).ToListAsync();
            return employeeAttendances;
        }

        public async Task InsertAsync(EmployeeAttendance entity)
        {
            await Context.EmployeeAttendance!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(EmployeeAttendance teacher)
        {
            var record = Context.Update(teacher);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            Context.EmployeeAttendance!.UpdateRange(employeeAttendances);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            Context.EmployeeAttendance!.RemoveRange(employeeAttendances);
            await Context.SaveChangesAsync();
        }

        public async Task InsertRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            Context.EmployeeAttendance!.AddRange(employeeAttendances);
            await Context.SaveChangesAsync();
        }
    }
}
