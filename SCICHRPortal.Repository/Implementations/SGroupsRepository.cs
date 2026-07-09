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

            // Get distinct descriptions first
            var distinctDescriptions = groups
                .Select(e => e.Description)
                .Distinct();

            // For each description, get the first group
            var query = distinctDescriptions
                .Select(desc => groups
                    .Where(e => e.Description == desc)
                    .OrderBy(e => e.Id)
                    .FirstOrDefault())
                .Where(e => e != null);

            var total = await query.CountAsync();

            var pagedGroups = query
                .OrderByDescending(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<SGroups>, int>(
                await pagedGroups.ToListAsync(),
                total
            );
        }

        public async Task<IEnumerable<SGroups>> GetAllAsync()
        {
            var groups = TimekeepingContext.SGroups!
                .Where(e => e.IsDeleted == false);


            // Get distinct descriptions first
            var distinctDescriptions = groups
                .Select(e => e.Description)
                .Distinct();

            // For each description, get the first group
            var query = distinctDescriptions
                .Select(desc => groups
                    .Where(e => e.Description == desc)
                    .OrderBy(e => e.Id)
                    .FirstOrDefault())
                .Where(e => e != null);

            return query!;
        }

        public async Task<SGroups> GetAsync(Guid id)
        {
            var group = await TimekeepingContext.SGroups!
                    .SingleOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            return group!;
        }
    }
}
