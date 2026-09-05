using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Repository.Implementations;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Implementations
{
    public class BiometricsLogService : IBiometricsLogService
    {
        private readonly IBiometricsLogRepository _repository;
        private readonly ILogger<BiometricsLogService> _logger;
        private const int EMPLOYEE_CHUNK_SIZE = 1000;

        public BiometricsLogService(
            IBiometricsLogRepository repository,
            ILogger<BiometricsLogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<BiometricsLog> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }
        // ============ EXISTING METHODS ============

        public async Task<IEnumerable<BiometricsLog>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<(IEnumerable<BiometricsLog>, int)> FilterAsync(
            int pageNumber,
            int pageSize,
            string? searchKeyword,
            DateTime? startDate,
            DateTime? endDate,
            string? deviceName)
        {
            return await _repository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate, endDate, deviceName);
        }

        public async Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? projectName)
        {
            return await _repository.FilterByProjectAndDateRange(startDate, endDate, projectName);
        }

        public async Task<BiometricsLog> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task InsertAsync(BiometricsLog biometricsLog)
        {
            await _repository.InsertAsync(biometricsLog);
        }

        public async Task<bool> UpdateAsync(BiometricsLog biometricsLog)
        {
            var existing = await _repository.GetByIdAsync(biometricsLog.BiometricsLogId);
            if (existing == null)
                return false;

            await _repository.UpdateAsync(biometricsLog);
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        // ============ TIMELOGS FROM TIMEKEEPING - FIXED ============

        public async Task<List<STimeLogs>> ImportDbDateRange(
            DateTime? startImport,
            DateTime? endImport,
            string? projectName)
        {
            return await _repository.GetTimeLogsFromTimekeepingAsync(startImport, endImport, projectName);
        }

        // ============ EMPLOYEE METHODS ============

        //public async Task<Employee> GetEmployeeAsync(string personnelNo)
        //{
        //    return await _repository.GetEmployeeFromDBAsync(personnelNo);
        //}

        public async Task<Dictionary<string, Employee>> GetEmployeesInBulkAsync(List<string> employeeNumbers)
        {
            var result = new Dictionary<string, Employee>();

            for (int i = 0; i < employeeNumbers.Count; i += EMPLOYEE_CHUNK_SIZE)
            {
                var chunk = employeeNumbers.Skip(i).Take(EMPLOYEE_CHUNK_SIZE).ToList();
                var employees = await _repository.GetEmployeesFromDBAsync(chunk);

                foreach (var employee in employees)
                {
                    if (!result.ContainsKey(employee.EmployeeNo))
                    {
                        result[employee.EmployeeNo] = employee;
                    }
                }
            }

            return result;
        }

        // ============ BULK IMPORT METHODS ============

        public async Task<int> BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();
            var result = await _repository.BulkInsertAsync(logs);
            stopwatch.Stop();

            _logger.LogInformation($"Bulk inserted {result} records in {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<(int Inserted, int Failed, List<string> Errors)> BulkInsertWithResultAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return (0, 0, new List<string>());

            var stopwatch = Stopwatch.StartNew();
            var result = await _repository.BulkInsertWithResultAsync(logs);
            stopwatch.Stop();

            _logger.LogInformation(
                $"Bulk insert completed: {result.Inserted} inserted, {result.Failed} failed in {stopwatch.ElapsedMilliseconds}ms"
            );

            return result;
        }

        public async Task<int> BulkInsertWithTransactionAsync(List<BiometricsLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();
            var result = await _repository.BulkInsertWithTransactionAsync(logs);
            stopwatch.Stop();

            _logger.LogInformation($"Bulk inserted {result} records with transaction in {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<int> BulkInsertWithProgressAsync(
            List<BiometricsLog> logs,
            IProgress<(int Processed, int Total, string Status)> progress)
        {
            if (logs == null || logs.Count == 0)
                return 0;

            var stopwatch = Stopwatch.StartNew();
            var result = await _repository.BulkInsertWithProgressAsync(logs, progress);
            stopwatch.Stop();

            _logger.LogInformation($"Bulk inserted {result} records with progress in {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}