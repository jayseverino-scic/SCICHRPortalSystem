using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Utility.Extensions;
using System.Data;
using System.Diagnostics;

namespace SCICHRPortal.Repository.Implementations
{
    public class BiometricsLogRepository : Repository, IBiometricsLogRepository
    {
        //private readonly ILogger<BiometricsBulkService> _logger;
        private const int BATCH_SIZE = 5000;
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
        public async Task BulkInsertAsync(IEnumerable<BiometricsLog> logs)
        {
            if (logs == null || !logs.Any())
                return;

            // Add range using EF Core
            await Context.BiometricsLog.AddRangeAsync(logs);
            await Context.SaveChangesAsync();
        }

        public async Task BulkInsertWithTransactionAsync(IEnumerable<BiometricsLog> logs)
        {
            if (logs == null || !logs.Any())
                return;

            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                await Context.BiometricsLog.AddRangeAsync(logs);
                await Context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> BulkInsertWithReturnCountAsync(IEnumerable<BiometricsLog> logs)
        {
            if (logs == null || !logs.Any())
                return 0;

            var logList = logs.ToList();
            await Context.BiometricsLog.AddRangeAsync(logList);
            await Context.SaveChangesAsync();

            return logList.Count;
        }
        public async Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            // Process in batches to avoid memory issues
            for (int i = 0; i < logs.Count; i += BATCH_SIZE)
            {
                var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();
                await Context.BiometricsLog.AddRangeAsync(batch);
                await Context.SaveChangesAsync();

                //_logger.LogDebug($"Batch {i / BATCH_SIZE + 1} inserted: {batch.Count} records");
            }

           // _logger.LogInformation($"Bulk inserted {logs.Count} records");
        }

        public async Task BulkInsertWithTransactionAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            using var transaction = await Context.Database.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < logs.Count; i += BATCH_SIZE)
                {
                    var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();
                    await Context.BiometricsLog.AddRangeAsync(batch);
                    await Context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                //_logger.LogInformation($"Bulk inserted {logs.Count} records with transaction");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BulkImportResult> BulkInsertWithResultAsync(List<BiometricsLog> logs)
        {
            var result = new BulkImportResult
            {
                StartTime = DateTime.Now,
                Errors = new List<string>()
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (logs == null || logs.Count == 0)
                {
                    result.TotalInserted = 0;
                    result.EndTime = DateTime.Now;
                    return result;
                }

                var batchCount = 0;
                var totalInserted = 0;

                for (int i = 0; i < logs.Count; i += BATCH_SIZE)
                {
                    batchCount++;
                    var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();

                    await Context.BiometricsLog.AddRangeAsync(batch);
                    var inserted = await Context.SaveChangesAsync();
                    totalInserted += inserted;

                    // Detach entities to free memory
                    foreach (var entity in batch)
                    {
                        Context.Entry(entity).State = EntityState.Detached;
                    }
                }

                result.TotalInserted = totalInserted;
                result.TotalFailed = logs.Count - totalInserted;
                result.BatchCount = batchCount;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.TotalFailed = logs?.Count ?? 0;
                //_logger.LogError(ex, "Error during bulk insert");
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                result.EndTime = DateTime.Now;
            }

            return result;
        }

        public async Task<BulkImportResult> BulkInsertWithProgressAsync(List<BiometricsLog> logs,IProgress<BulkProgress> progress)
        {
            var result = new BulkImportResult
            {
                StartTime = DateTime.Now,
                Errors = new List<string>()
            };

            if (logs == null || logs.Count == 0)
            {
                result.EndTime = DateTime.Now;
                return result;
            }

            var stopwatch = Stopwatch.StartNew();
            var totalProcessed = 0;
            var batchCount = 0;

            try
            {
                for (int i = 0; i < logs.Count; i += BATCH_SIZE)
                {
                    batchCount++;
                    var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();

                    progress?.Report(new BulkProgress
                    {
                        ProcessedRows = totalProcessed,
                        TotalRows = logs.Count,
                        Status = $"Processing batch {batchCount}"
                    });

                    await Context.BiometricsLog.AddRangeAsync(batch);
                    var inserted = await Context.SaveChangesAsync();
                    totalProcessed += inserted;

                    // Detach entities to free memory
                    foreach (var entity in batch)
                    {
                        Context.Entry(entity).State = EntityState.Detached;
                    }

                    //_logger.LogDebug($"Batch {batchCount} completed: {batch.Count} records");
                }

                result.TotalInserted = totalProcessed;
                result.TotalFailed = logs.Count - totalProcessed;
                result.BatchCount = batchCount;

                progress?.Report(new BulkProgress
                {
                    ProcessedRows = totalProcessed,
                    TotalRows = logs.Count,
                    Status = "Completed"
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.TotalFailed = logs.Count - totalProcessed;
               // _logger.LogError(ex, "Error during bulk insert with progress");
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                result.EndTime = DateTime.Now;
            }

            return result;
        }
    }
}
