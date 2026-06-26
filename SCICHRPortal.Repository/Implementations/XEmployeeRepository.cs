using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class XEmployeeRepository : Repository, IXEmployeeRepository
    {
        public XEmployeeRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
   : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<Tuple<IEnumerable<XEmployee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employees = XscribeContext.Employee!
              .Where(e => e._Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employees = employees
                    .Where(e =>
                        e.Last_Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            employees.Select(e => e.Department!).Load();
            employees.Select(e => e.Company_Position!).Load();
            employees.Select(e => e.Company_Branch!).Load();
            //employees.Include(e => e.Company_Position);
            //employees.Include(e => e.Department);
            //employees.Include(e => e.Company_Branch);

            var total = employees.Count();

            employees = employees
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<XEmployee>, int>(await employees.ToListAsync(), total);
        }

        public async Task<IEnumerable<XEmployee>> GetAllAsync()
        {
            var employees = await XscribeContext.Employee!.Where(s => !s._Deleted)
              .ToListAsync();
            return employees;
        }

        public async Task<XEmployee> GetAsync(int id)
        {
            var employee = await XscribeContext.Employee!
                    .SingleOrDefaultAsync(s => s.Id == id && !s._Deleted);
            return employee!;
        }
        public async Task<XEmployee> GetByEmployeeNoAsync(string employeeNo)
        {
            var employee = await XscribeContext.Employee!
                    .SingleOrDefaultAsync(s => s.Employee_code == employeeNo && !s._Deleted);
            return employee!;
        }
        public async Task<IEnumerable<XEmployee>> GetEmployeeByDepartment(int departmentId)
        {
            IEnumerable<XEmployee> employees = await XscribeContext.Employee!.Where(e => !e._Deleted).ToListAsync();
            if (departmentId > 0)
                employees = employees.Where(e => !e._Deleted && e.Department_Id == departmentId).ToList();

            return employees;
        }
    }
}
