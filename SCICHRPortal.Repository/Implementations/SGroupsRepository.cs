using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;


namespace SCICHRPortal.Repository.Implementations
{
    public class SGroupsRepository : Repository, ISGroupsRepository
    {
        public SGroupsRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }

        public async Task<Tuple<IEnumerable<SGroups>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var groups = TimekeepingContext.SGroups!
              .Where(e => e.IsDeleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                groups = groups
                    .Where(e =>
                        e.Description!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = groups.Select(e => e.Description).Distinct().Count();

            groups = groups
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<SGroups>, int>(await groups.ToListAsync(), total);
        }

        public async Task<IEnumerable<SGroups>> GetAllAsync()
        {
            var groups = await TimekeepingContext.SGroups!.Where(s => !s.IsDeleted)
              .ToListAsync();
            return groups;
        }

        public async Task<SGroups> GetAsync(Guid id)
        {
            var group = await TimekeepingContext.SGroups!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return group!;
        }
    }
}
