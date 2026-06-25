using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class XDepartmentRepository : Repository, IXDepartmentRepository
    {
        public XDepartmentRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<Tuple<IEnumerable<Department>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var departments = XscribeContext.Department!
              .Where(e => e._Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                departments = departments
                    .Where(e =>
                        e.Name!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = departments.Count();

            departments = departments
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Department>, int>(await departments.ToListAsync(), total);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            var departments = await XscribeContext.Department!.Where(s => !s._Deleted)
              .ToListAsync();
            return departments;
        }

        public async Task<Department> GetAsync(int id)
        {
            var department = await XscribeContext.Department!
                    .SingleOrDefaultAsync(s => s.Id == id && !s._Deleted);
            return department!;
        }
    }
}
