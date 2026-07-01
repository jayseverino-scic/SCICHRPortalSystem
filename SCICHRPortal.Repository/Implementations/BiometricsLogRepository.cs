using Microsoft.EntityFrameworkCore;
using System.Data;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.Repository.Implementations
{
    public class BiometricsLogRepository : Repository, IBiometricsLogRepository
    {

        public BiometricsLogRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
           
        }
        
        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            var biometricsLogs = Context.BiometricsLog.Where(b => b.Deleted == false && b.Date >= startDate && b.Date <= endDate && b.DeviceName == deviceName).AsNoTracking();

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

        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterPerProjectAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? projectName)
        {
            XCompany_Branch project = XscribeContext.Company_Branch!.Where(p => p.Name == projectName).FirstOrDefault();
            List<XEmployee> employees = XscribeContext.Employee!.Where(e => e.Company_Branch_Id == project!.Id).ToList();

            List<BiometricsLog> biometricsLogs1 = new List<BiometricsLog>();

            foreach (XEmployee employee in employees)
            {
                var biometricsLogsDB = Context.BiometricsLog.Where(b => b.Deleted == false && b.Date >= startDate && b.Date <= endDate && b.PersonnelId == "SCIC-" + employee.Id.ToString()).AsNoTracking().ToList();
                biometricsLogs1.AddRange(biometricsLogsDB);
            }

            IQueryable<BiometricsLog> biometricsLogs = biometricsLogs1.AsQueryable();
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
        public async Task<IEnumerable<BiometricsLog>> FilterByDateRange(DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            IEnumerable<BiometricsLog> biometricsLogs;
            biometricsLogs = await Context.BiometricsLog!.Where(b => !b.Deleted).ToListAsync();
            if (startDate.HasValue && endDate.HasValue)
                biometricsLogs = biometricsLogs.Where(b => b.Date >= startDate && b.Date <= endDate && b.DeviceName == deviceName);


            return biometricsLogs;
        }

        public async Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            IEnumerable<BiometricsLog> biometricsLogs;
            biometricsLogs = await Context.BiometricsLog!.Where(b => !b.Deleted).ToListAsync();
            if (startDate.HasValue && endDate.HasValue)
                biometricsLogs = biometricsLogs.Where(b => b.Date >= startDate && b.Date <= endDate && b.ProjectName == projectName);


            return biometricsLogs;
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
        public async Task<IEnumerable<TimeLogs>> ImportDbDateRange(DateTime? startDate, DateTime? endDate, string? serialNumber)
        {
            IEnumerable<TimeLogs> biometricsLogs;
            if (startDate.HasValue && endDate.HasValue)
                biometricsLogs = await TimekeepingContext.TimeLogs!.Where(b => b.RecordDate >= startDate && b.RecordDate <= endDate && b.DeviceSerialNumber == serialNumber).ToListAsync();
            else
                biometricsLogs = null;
            
            return biometricsLogs!;
        }


    }
}
