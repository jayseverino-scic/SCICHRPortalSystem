using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Repositories.Interfaces;
using SCICHRPortal.Repository;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BiometricsBulkService> _logger;
        private const int BATCH_SIZE = 5000;

        public BiometricsBulkService(
            ApplicationDbContext context,
            ILogger<BiometricsBulkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            // Process in batches to avoid memory issues
            for (int i = 0; i < logs.Count; i += BATCH_SIZE)
            {
                var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();
                await _context.BiometricsLogs.AddRangeAsync(batch);
                await _context.SaveChangesAsync();

                _logger.LogDebug($"Batch {i / BATCH_SIZE + 1} inserted: {batch.Count} records");
            }

            _logger.LogInformation($"Bulk inserted {logs.Count} records");
        }

        public async Task BulkInsertWithTransactionAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < logs.Count; i += BATCH_SIZE)
                {
                    var batch = logs.Skip(i).Take(BATCH_SIZE).ToList();
                    await _context.BiometricsLogs.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Bulk inserted {logs.Count} records with transaction");
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

                    await _context.BiometricsLogs.AddRangeAsync(batch);
                    var inserted = await _context.SaveChangesAsync();
                    totalInserted += inserted;

                    // Detach entities to free memory
                    foreach (var entity in batch)
                    {
                        _context.Entry(entity).State = EntityState.Detached;
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

                    await _context.BiometricsLogs.AddRangeAsync(batch);
                    var inserted = await _context.SaveChangesAsync();
                    totalProcessed += inserted;

                    // Detach entities to free memory
                    foreach (var entity in batch)
                    {
                        _context.Entry(entity).State = EntityState.Detached;
                    }

                    _logger.LogDebug($"Batch {batchCount} completed: {batch.Count} records");
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