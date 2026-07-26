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
            var biometricsLogs = Context.BiometricsLog.Where(b => b.Deleted == false && b.Date >= startDate && b.Date <= endDate && b.ProjectName == deviceName).AsNoTracking();

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
            var biometricsLogs = Context.BiometricsLog.Where(b => b.Deleted == false && b.Date >= startDate && b.Date <= endDate && b.ProjectName!.ToUpper() == projectName!.ToUpper()).AsNoTracking();

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
        public async Task<IEnumerable<STimeLogs>> ImportDbDateRange(DateTime? startDate, DateTime? endDate, string? serialNumber)
        {
            // Validate inputs
            if (!startDate.HasValue || !endDate.HasValue || string.IsNullOrEmpty(serialNumber))
            {
                return Enumerable.Empty<STimeLogs>();
            }

            var devices = TimekeepingContext.SGroups!
                .Where(e => e.Description != null && e.Description.ToUpper() == serialNumber.ToUpper())
                .ToList();

            if (!devices.Any())
            {
                return Enumerable.Empty<STimeLogs>();
            }

            var biometricsLogs = new List<STimeLogs>();

            foreach (var device in devices)
            {
                // Find the ZK device
                IEnumerable<SZKDevices> sZKDevices = TimekeepingContext.ZKDevices!.Where(e => e.Name.ToUpper() == device.Name.ToUpper());
                var sZKDevice = TimekeepingContext.ZKDevices!
                    .FirstOrDefault(e => e.Name != null && e.Name.ToUpper() == device.Name!.ToUpper());

                // Check if device exists and has a serial number
                if (sZKDevice == null || string.IsNullOrEmpty(sZKDevice.SerialNumber))
                {
                    continue; // Skip this device if no ZK device found
                }

                // Query logs for this specific device
                var logs = await TimekeepingContext.TimeLogs!
                    .Where(b => b.RecordDate >= startDate.Value &&
                                b.RecordDate <= endDate.Value &&
                                b.DeviceSerialNumber == sZKDevice.SerialNumber)
                    .ToListAsync();

                biometricsLogs.AddRange(logs);
            }

            return biometricsLogs;
        }


    }
}
