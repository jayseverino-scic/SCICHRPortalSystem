using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class XEmployeeRepository
: Repository, IXEmployeeRepository
    {
        public XEmployeeRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
   : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<Tuple<IEnumerable<Employee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employees = XscribeContext.Employee!
              .Where(e => e._Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employees = employees
                    .Where(e =>
                        e.Last_Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            employees.Select(e => e.Position).Load();
            employees.Select(e => e.Department).Load();

            var total = employees.Count();

            employees = employees
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Employee>, int>(await employees.ToListAsync(), total);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = await XscribeContext.Employee!.Where(s => !s._Deleted)
              .ToListAsync();
            return employees;
        }

        public async Task<Employee> GetAsync(int id)
        {
            var employee = await XscribeContext.Employee!
                    .SingleOrDefaultAsync(s => s.Id == id && !s._Deleted);
            return employee!;
        }
        public async Task<Employee> GetByEmployeeNoAsync(string employeeNo)
        {
            var employee = await XscribeContext.Employee!
                    .SingleOrDefaultAsync(s => s.Employee_code == employeeNo && !s._Deleted);
            return employee!;
        }
        public async Task<IEnumerable<Employee>> GetEmployeeByDepartment(int departmentId)
        {
            IEnumerable<Employee> employees = await XscribeContext.Employee!.Where(e => !e._Deleted).ToListAsync();
            if (departmentId > 0)
                employees = employees.Where(e => !e._Deleted && e.Department_Id == departmentId).ToList();

            return employees;
        }
    }
}
