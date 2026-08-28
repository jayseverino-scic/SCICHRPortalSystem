using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class SPersonnelsRepository
: Repository, ISPersonnelsRepository
    {
        public SPersonnelsRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
   : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<Tuple<IEnumerable<SPersonnels>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var employees = TimekeepingContext.SPersonnels!
              .Where(e => e.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                employees = employees
                    .Where(e =>
                        e.LastName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = employees.Count();

            employees = employees
                .OrderByDescending(e => e.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<SPersonnels>, int>(await employees.ToListAsync(), total);
        }

        public async Task<IEnumerable<SPersonnels>> GetAllAsync()
        {
            var employees = await TimekeepingContext.SPersonnels!.Where(s => !s.IsDeleted)
              .ToListAsync();
            return employees;
        }

        public async Task<SPersonnels> GetAsync(Guid id)
        {
            var employee = await TimekeepingContext.SPersonnels!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return employee!;
        }
        public async Task<SPersonnels> GetBySPersonnelsNoAsync(string employeeNo)
        {
            var employee = await TimekeepingContext.SPersonnels!
                    .SingleOrDefaultAsync(s => s.AccessNumber == employeeNo && !s.IsDeleted);
            return employee!;
        }
        public async Task<List<SPersonnels>> GetByMultiplePersonnelNoAsync(List<string> personnelNumbers)
        {
            if (personnelNumbers == null || !personnelNumbers.Any())
                return new List<SPersonnels>();

            return await TimekeepingContext.SPersonnels
                .Where(x => personnelNumbers.Contains(x.PersonnelNo!))
                .ToListAsync();
        }
    }
}
