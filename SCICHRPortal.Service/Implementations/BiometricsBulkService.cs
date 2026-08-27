using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Repositories.Interfaces;
using SCICHRPortal.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Implementations
{
    public class BiometricsBulkService : IBiometricsBulkService
    {
        private readonly IBiometricsBulkRepository _bulkRepository;
        private readonly ILogger<BiometricsBulkService> _logger;
        private const int BATCH_SIZE = 5000;

        public BiometricsBulkService(
            IBiometricsBulkRepository bulkRepository,
            ILogger<BiometricsBulkService> logger)
        {
            _bulkRepository = bulkRepository;
            _logger = logger;
        }

        /// <summary>
        /// Bulk insert biometrics logs
        /// </summary>
        public async Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            var dataTable = _bulkRepository.BuildDataTableFromLogs(logs);
            await _bulkRepository.BulkInsertAsync(dataTable);

            _logger.LogInformation($"Bulk inserted {logs.Count} records");
        }

        /// <summary>
        /// Bulk insert with transaction support
        /// </summary>
        public async Task BulkInsertWithTransactionAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            var dataTable = _bulkRepository.BuildDataTableFromLogs(logs);
            await _bulkRepository.BulkInsertWithTransactionAsync(dataTable);

            _logger.LogInformation($"Bulk inserted {logs.Count} records with transaction");
        }

        /// <summary>
        /// Bulk insert with result tracking
        /// </summary>
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

                var dataTable = _bulkRepository.BuildDataTableFromLogs(logs);
                var insertedCount = await _bulkRepository.BulkInsertWithReturnCountAsync(dataTable);

                result.TotalInserted = insertedCount;
                result.TotalFailed = logs.Count - insertedCount;
                result.BatchCount = (int)Math.Ceiling((double)logs.Count / BATCH_SIZE);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.TotalFailed = logs?.Count ?? 0;
                _logger.LogError(ex, "Error during bulk insert");
            }
            finally
            {
                stopwatch.Stop();
                result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                result.EndTime = DateTime.Now;
            }

            return result;
        }

        /// <summary>
        /// Bulk insert with progress reporting
        /// </summary>
        public async Task<BulkImportResult> BulkInsertWithProgressAsync(
            List<BiometricsLog> logs,
            IProgress<BulkProgress> progress)
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

            try
            {
                // Process in batches for progress reporting
                var batches = logs.Chunk(BATCH_SIZE);
                var batchCount = 0;

                foreach (var batch in batches)
                {
                    batchCount++;
                    var batchList = batch.ToList();

                    // Report progress
                    progress?.Report(new BulkProgress
                    {
                        ProcessedRows = totalProcessed,
                        TotalRows = logs.Count,
                        Status = $"Processing batch {batchCount}/{batches.Count()}"
                    });

                    var dataTable = _bulkRepository.BuildDataTableFromLogs(batchList);
                    await _bulkRepository.BulkInsertAsync(dataTable);

                    totalProcessed += batchList.Count;

                    _logger.LogDebug($"Batch {batchCount} completed: {batchList.Count} records");
                }

                result.TotalInserted = totalProcessed;
                result.TotalFailed = logs.Count - totalProcessed;
                result.BatchCount = batchCount;

                // Final progress
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
                _logger.LogError(ex, "Error during bulk insert with progress");
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