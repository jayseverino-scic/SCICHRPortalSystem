using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class EmployeeRepository
: Repository, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationContext context)
   : base(context)
        {
        }
        public async Task<bool> DeleteAsync(int EmployeeId)
        {
            var Employee = await Context.Employee!
                        .SingleOrDefaultAsync(s => s.EmployeeId == EmployeeId && !s.Deleted);
            if (Employee == null)
                return false;

            Employee.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Employee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employees = Context.Employee!
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employees = employees
                    .Where(e =>
                        e.LastName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            employees.Select(e => e.Position).Load();
            employees.Select(e => e.Department).Load();

            var total = employees.Count();

            employees = employees
                .OrderByDescending(e => e.EmployeeId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Employee>, int>(await employees.ToListAsync(), total);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = await Context.Employee!.Where(s => !s.Deleted)
              .ToListAsync();
            return employees;
        }

        public async Task<Employee> GetAsync(int id)
        {
            var employee = await Context.Employee!
                    .SingleOrDefaultAsync(s => s.EmployeeId == id && !s.Deleted);
            return employee!;
        }
        
        public async Task<Employee> GetByUserId(int userId)
        {
            var employee = await Context.Employee!.FirstOrDefaultAsync(t => t.UserId == userId && !t.Deleted);
            return employee!;
        }

        public async Task<IEnumerable<Employee>> GetEmployeeByDepartment(int departmentId)
        {
            IEnumerable<Employee> employees = await Context.Employee!.Where(e => !e.Deleted).ToListAsync();
            if (departmentId > 0)
                employees = employees.Where(e => !e.Deleted && e.DepartmentId == departmentId).ToList();

            return employees;
        }

        public async Task<DuplicateMessage> HasDuplicateName(Employee employee)
        {
            DuplicateMessage message = new();
            var title = employee.LastName!.ToLower().StringSplitThenJoin();
            var announcementMessage = employee.LastName!.ToLower().StringSplitThenJoin();
            var employees = await Context.Employee!
               .Where(r => r.Deleted == false).ToListAsync();

            var duplicatedTitle = employees.Any(t => t.LastName!.ToLower().StringSplitThenJoin() == title);
            var duplicatedMessage = employees.Any(t => announcementMessage.ToLower() == t.LastName!.ToLower().StringSplitThenJoin());
            var duplicatedDate = employees.Any(t => t.CreatedAt.Date == DateTime.Now.Date);

            if (duplicatedDate && duplicatedTitle)
            {
                message.Message = "Employee Last Name Duplicated";
            }
            else if (duplicatedDate && duplicatedMessage)
            {
                message.Message = "Employee Last Name Duplicated";
            }

            message.IsDuplicated = (duplicatedTitle || duplicatedMessage) && duplicatedDate;
            return message;
        }

        public async Task InsertAsync(Employee entity)
        {
            await Context.Employee!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            var record = Context.Update(employee);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
