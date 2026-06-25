using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeShiftDeviceRepository : Repository, IEmployeeShiftDeviceRepository
    {
        public EmployeeShiftDeviceRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext) : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var employeeShift = await Context.EmployeeShiftDevice!
                        .SingleOrDefaultAsync(s => s.AssignedShiftId == id && !s.Deleted);
            if (employeeShift == null)
                return false;

            employeeShift.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<EmployeeShiftDevice>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employeeShifts = Context.EmployeeShiftDevice!
                .Include(t => t.Employee)
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

            return new Tuple<IEnumerable<EmployeeShiftDevice>, int>(await employeeShifts.ToListAsync(), total);
        }

        public async Task<IEnumerable<EmployeeShiftDevice>> EmployeeShiftDeviceFilter(string? deviceName, int shiftId)
        {
            IEnumerable<EmployeeShiftDevice> employeeShifts;
            if (deviceName != "" && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShiftDevice!
                  .Include(t => t.Employee)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false  && e.Devicename == deviceName && e.ShiftId == shiftId).ToListAsync();
            }
            else if (deviceName != "" && shiftId == 0)
            {
                employeeShifts = await Context.EmployeeShiftDevice!
                  .Include(t => t.Employee)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.Devicename == deviceName).ToListAsync();
            }
            else if (deviceName == "" && shiftId != 0)
            {
                employeeShifts = await Context.EmployeeShiftDevice!
                  .Include(t => t.Employee)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false && e.ShiftId == shiftId).ToListAsync();
            }
            else
            {
                employeeShifts = await Context.EmployeeShiftDevice!
                  .Include(t => t.Employee)
                  .Include(t => t.Shift)
                  .Where(e => e.Deleted == false).ToListAsync();
            }
            return employeeShifts;
        }

        public async Task<EmployeeShiftDevice> GetAsync(int id)
        {
            var employeeShift = await Context.EmployeeShiftDevice!
                    .SingleOrDefaultAsync(s => s.AssignedShiftId == id && !s.Deleted);
            return employeeShift!;
        }

        public async Task<IEnumerable<EmployeeShiftDevice>> GetAllAsync()
        {
            var employeeShifts = await Context.EmployeeShiftDevice!.Where(e => !e.Deleted).ToListAsync();
            return employeeShifts;
        }
        public async Task<DuplicateMessage> HasDuplicateShift(EmployeeShiftDevice employeeShift)
        {
            DuplicateMessage message = new();
            var teachers = await Context.EmployeeShiftDevice!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicated = teachers.Any(t => t.EmployeeId == employeeShift.EmployeeId);

            if (duplicated)
            {
                message.Message = "Employee Shift Assignment Duplicated";
            }

            message.IsDuplicated = (duplicated);
            return message;
        }

        public async Task InsertAsync(EmployeeShiftDevice entity)
        {
            await Context.EmployeeShiftDevice!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(EmployeeShiftDevice teacher)
        {
            var record = Context.Update(teacher);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            Context.EmployeeShiftDevice!.UpdateRange(employeeShifts);
            await Context.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            Context.EmployeeShiftDevice!.RemoveRange(employeeShifts);
            await Context.SaveChangesAsync();
        }

        public async Task InsertRangeAsync(List<EmployeeShiftDevice> employeeShifts)
        {
            Context.EmployeeShiftDevice!.AddRange(employeeShifts);
            await Context.SaveChangesAsync();
        }
        public async Task<EmployeeShiftDevice> GetByEmployeeDevice(int id)
        {
            var employeeShift = await Context.EmployeeShiftDevice!
            .SingleOrDefaultAsync(s => s.EmployeeId == id && !s.Deleted);
            return employeeShift!;
        }
    }
}
