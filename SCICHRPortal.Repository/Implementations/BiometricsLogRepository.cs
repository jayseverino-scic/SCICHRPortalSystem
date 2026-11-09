using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using System.Data;

namespace SCICHRPortal.Repository.Implementations
{
    public partial class BiometricsLogRepository : Repository, IBiometricsLogRepository
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
                biometricsLog.ImportSource = existing.ImportSource;
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

        public async Task<(List<STimeLogs> Logs, Dictionary<string, string> DeviceAliases)> GetTimeLogsFromTimekeepingAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? projectName)
        {
            // Validate inputs
            if (!startDate.HasValue || !endDate.HasValue || string.IsNullOrEmpty(projectName))
            {
                return ([], new(StringComparer.OrdinalIgnoreCase));
            }

            var project = projectName.ToUpper();
            var deviceNames = await TimekeepingContext.SGroups
                .Where(e => e.Description != null && e.Description.ToUpper() == project && e.Name != null)
                .Select(e => e.Name ?? string.Empty)
                .ToListAsync();
            var biometricsLogs = new List<STimeLogs>();
            var deviceAliases = new List<(string Alias, string Serial)>();

            foreach (var deviceName in deviceNames)
            {
                var name = deviceName.ToUpper();
                var serialNumbers = await TimekeepingContext.ZKDevices
                    .Where(e => e.Name != null && e.Name.ToUpper() == name)
                    .Select(e => e.SerialNumber)
                    .ToListAsync();
                // Older imports stored a device name or project label instead of the serial number.
                foreach (var serial in serialNumbers)
                {
                    if (string.IsNullOrWhiteSpace(serial)) continue;
                    deviceAliases.Add((serial.Trim(), serial));
                    deviceAliases.Add((deviceName.Trim(), serial));
                    deviceAliases.Add((projectName.Trim(), serial));
                }

                var serialNumber = serialNumbers.FirstOrDefault();
                if (string.IsNullOrEmpty(serialNumber))
                {
                    continue; // Skip this device if no ZK device found
                }

                // Query logs for this specific device
                var logs = await TimekeepingContext.TimeLogs!
                    .Where(b => b.RecordDate >= startDate.Value &&
                                b.RecordDate <= endDate.Value &&
                                b.DeviceSerialNumber == serialNumber)
                    .ToListAsync();

                biometricsLogs.AddRange(logs);
            }

            var uniqueAliases = deviceAliases.Where(device => device.Alias.Length > 0)
                .GroupBy(device => device.Alias, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Select(device => device.Serial).Distinct().Take(2).Count() == 1)
                .ToDictionary(group => group.Key, group => group.First().Serial, StringComparer.OrdinalIgnoreCase);
            return (biometricsLogs, uniqueAliases);
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

            await using var writer = await connection.BeginBinaryImportAsync("""
                COPY "BiometricsLog" ("PersonnelId", "LastName", "FirstName", "Date", "Time", "LogType", "DeviceName",
                    "ProjectName", "CreatedAt", "CreatedBy", "ImportSource") FROM STDIN (FORMAT BINARY)
                """);

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
                await writer.WriteAsync(log.ImportSource, NpgsqlTypes.NpgsqlDbType.Text);
            }

            await writer.CompleteAsync();
            return logs.Count;
        }

        private async Task<int> BulkInsertNewPunchesAsync(
            List<BiometricsLog> logs, IReadOnlyDictionary<string, string>? deviceAliases)
        {
            var punches = logs.OrderBy(log => log.CreatedAt)
                .DistinctBy(GetPunchKey)
                .ToList();
            var personnelIds = punches.Select(log => log.PersonnelId ?? string.Empty).Distinct().ToArray();
            var dates = punches.Select(log => ToPostgresTimestampPrecision(log.Date)).Distinct().ToArray();
            var projects = punches.Select(log => log.ProjectName?.ToUpperInvariant() ?? string.Empty).Distinct().ToArray();
            var aliases = deviceAliases?.ToDictionary(device => device.Key, device => device.Value, StringComparer.OrdinalIgnoreCase);

            const int maxAttempts = 3;
            for (var attempt = 1; ; attempt++)
            {
                List<BiometricsLog> newLogs = [];
                try
                {
                    await using var transaction = await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                    var existing = await Context.BiometricsLog.AsNoTracking()
                        .Where(row => !row.Deleted && row.PersonnelId != null && personnelIds.Contains(row.PersonnelId)
                            && row.Date.HasValue && dates.Contains(row.Date) && row.Time.HasValue
                            && row.ProjectName != null && projects.Contains(row.ProjectName.ToUpper()))
                        .Select(row => new PunchKey(row.PersonnelId ?? string.Empty, row.Date, row.Time,
                            row.LogType ?? string.Empty, row.DeviceName ?? string.Empty,
                            (row.ProjectName ?? string.Empty).ToUpper()))
                        .ToListAsync();
                    var existingKeys = existing.ToHashSet();
                    var aliasedKeys = new HashSet<PunchKey>();
                    foreach (var key in existing)
                    {
                        if (aliases != null && aliases.TryGetValue(key.DeviceName.Trim(), out var serial))
                            aliasedKeys.Add(key with { DeviceName = serial.ToUpperInvariant() });
                    }

                    foreach (var log in punches)
                    {
                        var key = GetPunchKey(log);
                        if (existingKeys.Contains(key) || aliasedKeys.Contains(key with { DeviceName = key.DeviceName.ToUpperInvariant() }))
                            continue;

                        newLogs.Add(new BiometricsLog
                        {
                            PersonnelId = key.PersonnelId,
                            LastName = log.LastName ?? string.Empty,
                            FirstName = log.FirstName ?? string.Empty,
                            Date = key.Date,
                            Time = key.Time,
                            LogType = key.LogType,
                            DeviceName = key.DeviceName,
                            ProjectName = log.ProjectName ?? string.Empty,
                            CreatedAt = log.CreatedAt,
                            CreatedBy = log.CreatedBy ?? string.Empty,
                            ImportSource = log.ImportSource
                        });
                    }

                    if (newLogs.Count == 0) return 0;

                    Context.BiometricsLog.AddRange(newLogs);
                    // Use the EF save overload to preserve the import's existing audit behavior.
                    await Context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                    await transaction.CommitAsync();
                    return newLogs.Count;
                }
                catch (Exception exception) when (attempt < maxAttempts &&
                    exception.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure })
                {
                    // A competing import committed first. Check for its punches in a new transaction.
                }
                finally
                {
                    foreach (var log in newLogs)
                        Context.Entry(log).State = EntityState.Detached;
                }
            }
        }

        private static PunchKey GetPunchKey(BiometricsLog log) => new(
            log.PersonnelId ?? string.Empty, ToPostgresTimestampPrecision(log.Date),
            ToPostgresTimestampPrecision(log.Time), log.LogType ?? string.Empty,
            log.DeviceName ?? string.Empty, log.ProjectName?.ToUpperInvariant() ?? string.Empty);

        private static DateTime? ToPostgresTimestampPrecision(DateTime? value)
        {
            if (value is not { } timestamp || timestamp == DateTime.MinValue || timestamp == DateTime.MaxValue)
                return value;

            // PostgreSQL stores whole microseconds relative to its 2000-01-01 epoch.
            var ticksFromEpoch = (timestamp - new DateTime(2000, 1, 1)).Ticks;
            return timestamp.AddTicks(-(ticksFromEpoch % TimeSpan.TicksPerMicrosecond));
        }

        private readonly record struct PunchKey(
            string PersonnelId, DateTime? Date, DateTime? Time, string LogType, string DeviceName, string ProjectName);

        /// <summary>
        /// Inserts biometric logs and reports the outcome.
        /// </summary>
        public async Task<(int Inserted, int Failed, List<string> Errors)> BulkInsertWithResultAsync(
            List<BiometricsLog> logs, bool skipExistingPunches = false, IReadOnlyDictionary<string, string>? deviceAliases = null)
        {
            var errors = new List<string>();
            var inserted = 0;
            var failed = 0;

            if (logs == null || logs.Count == 0)
                return (0, 0, errors);

            try
            {
                inserted = skipExistingPunches
                    ? await BulkInsertNewPunchesAsync(logs, deviceAliases)
                    : await BulkInsertAsync(logs);
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
                    "COPY \"BiometricsLog\" (\"PersonnelId\", \"LastName\", \"FirstName\", \"Date\", \"Time\", \"LogType\", \"DeviceName\", \"ProjectName\", \"CreatedAt\", \"CreatedBy\", \"ImportSource\") FROM STDIN (FORMAT BINARY)"
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
                await writer.WriteAsync(log.ImportSource, NpgsqlTypes.NpgsqlDbType.Text);
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
                "COPY \"BiometricsLog\" (\"PersonnelId\", \"LastName\", \"FirstName\", \"Date\", \"Time\", \"LogType\", \"DeviceName\", \"ProjectName\", \"CreatedAt\", \"CreatedBy\", \"ImportSource\") FROM STDIN (FORMAT BINARY)"
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
                await writer.WriteAsync(log.ImportSource, NpgsqlTypes.NpgsqlDbType.Text);

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
