using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SCICHRPortal.Data;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.Repository.Implementations
{
    public class BiometricsLogRepository : Repository, IBiometricsLogRepository
    {
        private readonly string _connectionString;
        private const int BULK_BATCH_SIZE = 5000;
        private const int BULK_TIMEOUT = 300;

        public BiometricsLogRepository(
            ApplicationContext context,
            XscribeContext xscribeContext,
            TimekeepingContext timekeepingContext,
            IConfiguration configuration)
            : base(context, xscribeContext, timekeepingContext)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("DefaultConnection connection string not found in configuration.");
            }
        }

        // ============ EXISTING CRUD METHODS ============
        public async Task<BiometricsLog> GetAsync(int id)
        {
            var biometricsLog = await Context.BiometricsLog!
                    .SingleOrDefaultAsync(s => s.BiometricsLogId == id && !s.Deleted);
            return biometricsLog!;
        }
        public async Task<IEnumerable<BiometricsLog>> GetAllAsync()
        {
            return await Context.BiometricsLog
                .OrderByDescending(x => x.BiometricsLogId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<BiometricsLog>, int)> FilterAsync(
            int pageNumber,
            int pageSize,
            string? searchKeyword,
            DateTime? startDate,
            DateTime? endDate,
            string? deviceName)
        {
            var query = Context.BiometricsLog.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(x =>
                    x.PersonnelId.Contains(searchKeyword) ||
                    (x.LastName != null && x.LastName.Contains(searchKeyword)) ||
                    (x.FirstName != null && x.FirstName.Contains(searchKeyword)));
            }

            if (startDate.HasValue)
                query = query.Where(x => x.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.Date <= endDate.Value);

            if (!string.IsNullOrEmpty(deviceName))
                query = query.Where(x => x.DeviceName == deviceName);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.BiometricsLogId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? projectName)
        {
            var query = Context.BiometricsLog.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(x => x.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.Date <= endDate.Value);

            if (!string.IsNullOrEmpty(projectName))
                query = query.Where(x => x.ProjectName == projectName);

            return await query
                .OrderByDescending(x => x.BiometricsLogId)
                .ToListAsync();
        }

        public async Task<BiometricsLog> GetByIdAsync(int id)
        {
            return await Context.BiometricsLog
                .FirstOrDefaultAsync(x => x.BiometricsLogId == id);
        }

        public async Task InsertAsync(BiometricsLog biometricsLog)
        {
            await Context.BiometricsLog.AddAsync(biometricsLog);
            await Context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BiometricsLog biometricsLog)
        {
            var existing = await Context.BiometricsLog.FindAsync(biometricsLog.BiometricsLogId);
            if (existing != null)
            {
                Context.Entry(existing).CurrentValues.SetValues(biometricsLog);
                existing.UpdatedAt = DateTime.Now;
                await Context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await Context.BiometricsLog.FindAsync(id);
            if (entity != null)
            {
                Context.BiometricsLog.Remove(entity);
                await Context.SaveChangesAsync();
            }
        }

        // ============ TIMELOGS FROM TIMEKEEPING CONTEXT - FIXED ============

        public async Task<List<STimeLogs>> GetTimeLogsFromTimekeepingAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? projectName)
        {
            // Validate inputs
            if (!startDate.HasValue || !endDate.HasValue || string.IsNullOrEmpty(projectName))
            {
                return new List<STimeLogs>();
            }

            var devices = TimekeepingContext.SGroups!
                .Where(e => e.Description != null && e.Description.ToUpper() == projectName.ToUpper())
                .ToList();

            if (!devices.Any())
            {
                return new List<STimeLogs>();
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
            //var query = TimekeepingContext.TimeLogs.AsQueryable();

            //if (startDate.HasValue)
            //    query = query.Where(x => x.RecordDate >= startDate.Value);

            //if (endDate.HasValue)
            //    query = query.Where(x => x.RecordDate <= endDate.Value);
            //if (!string.IsNullOrEmpty(serialNumber))
            //{
            //    var devices = TimekeepingContext.SGroups!.Where(d => d.Description!.ToUpper() == serialNumber!.ToUpper()).ToList();
            //    if (devices != null)
            //    {

            //    }
            //    query = query.Where(x => x.DeviceSerialNumber == serialNumber);
            //}

            //return await query.ToListAsync();
        }

        // ============ EMPLOYEES FROM XSCRIBE CONTEXT ============

        public async Task<List<Employee>> GetEmployeesDBAsync(List<string> personnelNumbers)
        {
            if (personnelNumbers == null || !personnelNumbers.Any())
                return new List<Employee>();

            // For PostgreSQL with EF Core, use Contains with parameterized query
            // This works with Npgsql EF Core provider
            try
            {
                // Method 1: Use Contains with array (works with Npgsql)
                return await Context.Employee
                    .Where(x => personnelNumbers.Contains(x.EmployeeNo))
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Method 2: Use Any with array (alternative approach)
                try
                {
                    return await Context.Employee
                        .Where(x => personnelNumbers.Any(p => p == x.EmployeeNo))
                        .ToListAsync();
                }
                catch (Exception)
                {
                    // Method 3: Use raw SQL with ANY operator (PostgreSQL specific)
                    return await GetEmployeesFromDBAsync(personnelNumbers);
                }
            }
        }
        public async Task<List<Employee>> GetEmployeesFromDBAsync(List<string> personnelNumbers)
        {
            if (personnelNumbers == null || !personnelNumbers.Any())
                return new List<Employee>();

            // Create a PostgreSQL array parameter
            var param = new NpgsqlParameter("@PersonnelNumbers", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = personnelNumbers.ToArray()
            };

            var sql = "SELECT * FROM \"Employee\" WHERE \"EmployeeNo\" = ANY(@PersonnelNumbers)";

            return await Context.Employee
                .FromSqlRaw(sql, param)
                .ToListAsync();
        }

        /// <summary>
        /// Alternative: Use string join for PostgreSQL
        /// </summary>
        private async Task<List<SPersonnels>> GetEmployeesFromXscribeViaStringJoinAsync(List<string> personnelNumbers)
        {
            if (personnelNumbers == null || !personnelNumbers.Any())
                return new List<SPersonnels>();

            // Using string_agg or array for PostgreSQL
            var quotedValues = string.Join(",", personnelNumbers.Select(p => $"'{p.Replace("'", "''")}'"));

            var sql = $"SELECT * FROM \"SPersonnels\" WHERE \"PersonnelNo\" IN ({quotedValues})";

            return await TimekeepingContext.SPersonnels
                .FromSqlRaw(sql)
                .ToListAsync();
        }


        // ============ BULK IMPORT METHODS ============

        public async Task<int> BulkInsertAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport(
                "COPY \"BiometricsLog\" (\"PersonnelId\", \"LastName\", \"FirstName\", \"Date\", \"Time\", \"LogType\", \"DeviceName\", \"ProjectName\", \"CreatedAt\", \"CreatedBy\") FROM STDIN (FORMAT BINARY)"
            );

            foreach (var log in logs)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(log.PersonnelId ?? string.Empty);
                await writer.WriteAsync(log.LastName ?? string.Empty);
                await writer.WriteAsync(log.FirstName ?? string.Empty);
                await writer.WriteAsync(log.Date, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.Time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.LogType ?? string.Empty);
                await writer.WriteAsync(log.DeviceName ?? string.Empty);
                await writer.WriteAsync(log.ProjectName ?? string.Empty);
                await writer.WriteAsync(log.CreatedAt, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.CreatedBy ?? string.Empty);
            }

            await writer.CompleteAsync();
            return logs.Count;
        }

        /// <summary>
        /// PostgreSQL COPY with result tracking
        /// </summary>
        public async Task<(int Inserted, int Failed, List<string> Errors)> BulkInsertWithResultAsync(List<BiometricsLog> logs)
        {
            var errors = new List<string>();
            var inserted = 0;
            var failed = 0;

            if (logs == null || logs.Count == 0)
                return (0, 0, errors);

            try
            {
                inserted = await BulkInsertAsync(logs);
            }
            catch (Exception ex)
            {
                failed = logs.Count;
                errors.Add($"Bulk insert failed: {ex.Message}");
                if (ex.InnerException != null)
                    errors.Add($"Inner exception: {ex.InnerException.Message}");
            }

            return (inserted, failed, errors);
        }

        /// <summary>
        /// PostgreSQL COPY with transaction support
        /// </summary>
        public async Task<int> BulkInsertWithTransactionAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                using var writer = connection.BeginBinaryImport(
                    "COPY \"BiometricsLog\" (\"PersonnelId\", \"LastName\", \"FirstName\", \"Date\", \"Time\", \"LogType\", \"DeviceName\", \"ProjectName\", \"CreatedAt\", \"CreatedBy\") FROM STDIN (FORMAT BINARY)"
                );

                foreach (var log in logs)
                {
                    await writer.StartRowAsync();
                    await writer.WriteAsync(log.PersonnelId ?? string.Empty);
                    await writer.WriteAsync(log.LastName ?? string.Empty);
                    await writer.WriteAsync(log.FirstName ?? string.Empty);
                    await writer.WriteAsync(log.Date, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(log.Time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(log.LogType ?? string.Empty);
                    await writer.WriteAsync(log.DeviceName ?? string.Empty);
                    await writer.WriteAsync(log.ProjectName ?? string.Empty);
                    await writer.WriteAsync(log.CreatedAt, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(log.CreatedBy ?? string.Empty);
                }

                await writer.CompleteAsync();
                await transaction.CommitAsync();

                return logs.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// PostgreSQL COPY with progress tracking
        /// </summary>
        public async Task<int> BulkInsertWithProgressAsync(
            List<BiometricsLog> logs,
            IProgress<(int Processed, int Total, string Status)> progress)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            var totalInserted = 0;
            var totalProcessed = 0;

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var writer = connection.BeginBinaryImport(
                "COPY \"BiometricsLog\" (\"PersonnelId\", \"LastName\", \"FirstName\", \"Date\", \"Time\", \"LogType\", \"DeviceName\", \"ProjectName\", \"CreatedAt\", \"CreatedBy\") FROM STDIN (FORMAT BINARY)"
            );

            int batchCount = 0;
            int totalBatches = (int)Math.Ceiling((double)logs.Count / BULK_BATCH_SIZE);

            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];

                await writer.StartRowAsync();
                await writer.WriteAsync(log.PersonnelId ?? string.Empty);
                await writer.WriteAsync(log.LastName ?? string.Empty);
                await writer.WriteAsync(log.FirstName ?? string.Empty);
                await writer.WriteAsync(log.Date, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.Time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.LogType ?? string.Empty);
                await writer.WriteAsync(log.DeviceName ?? string.Empty);
                await writer.WriteAsync(log.ProjectName ?? string.Empty);
                await writer.WriteAsync(log.CreatedAt, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.CreatedBy ?? string.Empty);

                totalProcessed++;

                // Report progress every batch
                if (totalProcessed % BULK_BATCH_SIZE == 0 || totalProcessed == logs.Count)
                {
                    batchCount++;
                    progress?.Report((
                        totalProcessed,
                        logs.Count,
                        $"Processing batch {batchCount}/{totalBatches}"
                    ));
                }
            }

            await writer.CompleteAsync();
            totalInserted = logs.Count;

            progress?.Report((
                totalInserted,
                logs.Count,
                "Completed"
            ));

            return totalInserted;
        }

    }
}