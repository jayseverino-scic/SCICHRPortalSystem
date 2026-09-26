using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class HolidayRepository : Repository, IHolidayRepository
    {
        public HolidayRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
        }
        public async Task<bool> DeleteAsync(int holidayId)
        {
            var holiday = await Context.Holiday!
                        .SingleOrDefaultAsync(s => s.HolidayId == holidayId && !s.Deleted);
            if (holiday == null)
                return false;

            holiday.Deleted = true;
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            var holidays = Context.Holiday!.AsNoTracking()
              .Include(e => e.Projects)
              .Where(e => e.Deleted == false);

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                holidays = holidays
                    .Where(e =>
                        e.HolidayName!.ToLower().Contains(searchKeyword.ToLower()));
            }

            var total = await holidays.CountAsync();

            holidays = holidays
                .OrderByDescending(e => e.HolidayId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<Holiday>, int>(await holidays.ToListAsync(), total);
        }

        public async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            var holidays = await Context.Holiday!.AsNoTracking().Where(s => !s.Deleted)
              .Include(e => e.Projects)
              .ToListAsync();
            return holidays;
        }

        public async Task<Holiday> GetAsync(int id)
        {
            var holiday = await Context.Holiday!.AsNoTracking()
                    .Include(e => e.Projects)
                    .SingleOrDefaultAsync(s => s.HolidayId == id && !s.Deleted);
            return holiday!;
        }

        public async Task InsertAsync(Holiday holiday, IReadOnlyCollection<int> projectIds)
        {
            Context.Holiday!.Add(holiday);
            foreach (var projectId in projectIds.Distinct())
                Context.HolidayProject.Add(new HolidayProject {
                    Holiday = holiday, ProjectId = projectId,
                    CreatedAt = holiday.CreatedAt, CreatedBy = holiday.CreatedBy });
            await Context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Holiday holiday, IReadOnlyCollection<int> projectIds)
        {
            var tracked = await Context.Holiday!
                .SingleAsync(existing => existing.HolidayId == holiday.HolidayId && !existing.Deleted);
            tracked.HolidayName = holiday.HolidayName;
            tracked.HolidayDate = holiday.HolidayDate;
            tracked.HolidayType = holiday.HolidayType;
            tracked.UpdatedAt = holiday.UpdatedAt;
            tracked.UpdatedBy = holiday.UpdatedBy;

            // Keep the audited join history separate from the public Projects read model.
            // Removing skip-navigation entries would request physical join deletion.
            var links = await Context.HolidayProject.IgnoreQueryFilters()
                .Where(link => link.HolidayId == tracked.HolidayId).ToListAsync();
            var selectedIds = projectIds.ToHashSet();
            foreach (var link in links)
            {
                var removed = !selectedIds.Contains(link.ProjectId);
                if (link.Deleted == removed)
                    continue;
                link.Deleted = removed;
                link.UpdatedAt = holiday.UpdatedAt;
                link.UpdatedBy = holiday.UpdatedBy;
            }
            var knownIds = links.Select(link => link.ProjectId).ToHashSet();
            foreach (var projectId in selectedIds.Except(knownIds))
                Context.HolidayProject.Add(new HolidayProject {
                    Holiday = tracked, ProjectId = projectId,
                    CreatedAt = holiday.UpdatedAt ?? holiday.CreatedAt, CreatedBy = holiday.UpdatedBy });

            await Context.SaveChangesAsync();
        }

    }
}

