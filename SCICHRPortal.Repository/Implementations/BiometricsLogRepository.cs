using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Repository.Implementations
{
    public class BiometricsLogRepository : Repository, IBiometricsLogRepository
    {
        public BiometricsLogRepository(ApplicationContext context)
    : base(context)
        {
        }

        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate)
        {
            var biometricsLogs = Context.BiometricsLog.Where(b => b.Deleted == false);
            if (startDate.HasValue && endDate.HasValue)
                biometricsLogs = biometricsLogs.Where(b => b.Date >= startDate && b.Date <= endDate).AsNoTracking();

            if (!String.IsNullOrWhiteSpace(searchKeyword))
            {
                //items = items
                //    .Where(e =>
                //        e.Date!.ToLower().Contains(searchKeyword.ToLower()));
            }
            var total = biometricsLogs.Count();

            biometricsLogs = biometricsLogs
                .OrderByDescending(e => e.BiometricsLogId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return new Tuple<IEnumerable<BiometricsLog>, int>(await biometricsLogs.ToListAsync(), total);
        }
        public async Task<IEnumerable<BiometricsLog>> GetDailyLogAsync(DateTime logDate)
        {
            IEnumerable<BiometricsLog> biometricsLogs;

            biometricsLogs = await Context.BiometricsLog!
                .Where(e => e.Deleted == false && e.Date == logDate).ToListAsync();

            return biometricsLogs;
        }
        public async Task<IEnumerable<BiometricsLog>> GetAllAsync()
        {
            var biometricsLogs = await Context.BiometricsLog!.Where(s => !s.Deleted)
              .ToListAsync();
            return biometricsLogs;
        }

        public async Task<BiometricsLog> GetAsync(int id)
        {
            var item = await Context.BiometricsLog!
                    .SingleOrDefaultAsync(s => s.BiometricsLogId == id && !s.Deleted);
            return item!;
        }

        public async Task InsertAsync(BiometricsLog entity)
        {
            await Context.BiometricsLog!.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(BiometricsLog biometricsLog)
        {
            var record = Context.Update(biometricsLog);
            if (record is null)
                return false;

            await Context.SaveChangesAsync();
            return true;
        }
    }
}
