using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SCICHRPortal.Repository.Implementations
{
    public class BiometricsBulkRepository : Repository, IBiometricsBulkRepository
    {
        private const int BULK_BATCH_SIZE = 5000;
        private const int BULK_TIMEOUT = 300; // 5 minutes

        public BiometricsBulkRepository(ApplicationContext context, XscribeContext xscribeContext, TimekeepingContext timekeepingContext)
    : base(context, xscribeContext, timekeepingContext)
        {
            
        }

        /// <summary>
        /// Bulk insert using SqlBulkCopy
        /// </summary>
        public async Task BulkInsertAsync(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;

            using var connection = new SqlConnection(Context);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "BiometricsLog",
                BatchSize = BULK_BATCH_SIZE,
                BulkCopyTimeout = BULK_TIMEOUT,
                EnableStreaming = true
            };

            // Map columns
            MapColumns(bulkCopy);

            await bulkCopy.WriteToServerAsync(dataTable);
        }

        /// <summary>
        /// Bulk insert with transaction support
        /// </summary>
        public async Task BulkInsertWithTransactionAsync(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var bulkCopy = new SqlBulkCopy(
                    connection,
                    SqlBulkCopyOptions.Default,
                    transaction)
                {
                    DestinationTableName = "BiometricsLogs",
                    BatchSize = BULK_BATCH_SIZE,
                    BulkCopyTimeout = BULK_TIMEOUT,
                    EnableStreaming = true
                };

                // Map columns
                MapColumns(bulkCopy);

                await bulkCopy.WriteToServerAsync(dataTable);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Bulk insert with return count
        /// </summary>
        public async Task<int> BulkInsertWithReturnCountAsync(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return 0;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "BiometricsLogs",
                BatchSize = BULK_BATCH_SIZE,
                BulkCopyTimeout = BULK_TIMEOUT,
                EnableStreaming = true
            };

            // Map columns
            MapColumns(bulkCopy);

            await bulkCopy.WriteToServerAsync(dataTable);
            return dataTable.Rows.Count;
        }

        /// <summary>
        /// Build DataTable from BiometricsLog list
        /// </summary>
        public DataTable BuildDataTableFromLogs(List<BiometricsLog> logs)
        {
            var table = new DataTable("BiometricsLogs");

            // Define columns matching your database table
            table.Columns.Add("PersonnelId", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Time", typeof(DateTime));
            table.Columns.Add("LogType", typeof(string));
            table.Columns.Add("DeviceName", typeof(string));
            table.Columns.Add("ProjectName", typeof(string));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("CreatedBy", typeof(string));

            // Add rows
            foreach (var log in logs)
            {
                var row = table.NewRow();
                row["PersonnelId"] = log.PersonnelId ?? string.Empty;
                row["LastName"] = log.LastName ?? string.Empty;
                row["FirstName"] = log.FirstName ?? string.Empty;
                row["Date"] = log.Date;
                row["Time"] = log.Time;
                row["LogType"] = log.LogType ?? string.Empty;
                row["DeviceName"] = log.DeviceName ?? string.Empty;
                row["ProjectName"] = log.ProjectName ?? string.Empty;
                row["CreatedAt"] = log.CreatedAt;
                row["CreatedBy"] = log.CreatedBy ?? string.Empty;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Map columns for SqlBulkCopy
        /// </summary>
        private void MapColumns(SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Add("PersonnelId", "PersonnelId");
            bulkCopy.ColumnMappings.Add("LastName", "LastName");
            bulkCopy.ColumnMappings.Add("FirstName", "FirstName");
            bulkCopy.ColumnMappings.Add("Date", "Date");
            bulkCopy.ColumnMappings.Add("Time", "Time");
            bulkCopy.ColumnMappings.Add("LogType", "LogType");
            bulkCopy.ColumnMappings.Add("DeviceName", "DeviceName");
            bulkCopy.ColumnMappings.Add("ProjectName", "ProjectName");
            bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");
            bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
        }
    }
}