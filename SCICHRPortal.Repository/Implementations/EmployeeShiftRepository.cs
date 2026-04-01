using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeShiftRepository : Repository, IEmployeeShiftRepository
    {
        public EmployeeShiftRepository(ApplicationContext context) : base(context)
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
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false  && e.DepartmentId == departmentId && e.ShiftId == shiftId).ToListAsync();
            }
            else if (departmentId != 0 && shiftId == 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.DepartmentId == departmentId).ToListAsync();
            }
            else if (departmentId == 0 && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await Context.EmployeeShift!
                  .Include(t => t.Employee)
                  .Include(t => t.Department)
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
            await Context.EmployeeShift!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(EmployeeShift teacher)
        {
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
    }
}
