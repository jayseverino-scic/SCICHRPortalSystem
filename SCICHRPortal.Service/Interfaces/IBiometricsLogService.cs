using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IBiometricsLogService : IScopedService,
        IInserter<BiometricsLog>,
         IRetriever<BiometricsLog, int>,
         IListRetriever<BiometricsLog>
    {
        // Existing methods
        Task<IEnumerable<BiometricsLog>> GetAllAsync();
        Task<(IEnumerable<BiometricsLog>, int)> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName);
        Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName);
        Task<BiometricsLog> GetByIdAsync(int id);
        Task InsertAsync(BiometricsLog biometricsLog);
        Task<bool> UpdateAsync(BiometricsLog biometricsLog);
        Task DeleteAsync(int id);

        // Time logs from database - FIXED: Use concrete type instead of dynamic
        Task<List<STimeLogs>> ImportDbDateRange(DateTime? startImport, DateTime? endImport, string? projectName);

        // Employees from Xscribe
        //Task<XEmployee> GetEmployeeAsync(string personnelNo);
        Task<Dictionary<string, Employee>> GetEmployeesInBulkAsync(List<string> employeeNumbers);

        // Bulk import methods
        Task<int> BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs);
        Task<(int Inserted, int Failed, List<string> Errors)> BulkInsertWithResultAsync(List<BiometricsLog> logs);
        Task<int> BulkInsertWithTransactionAsync(List<BiometricsLog> logs);
        Task<int> BulkInsertWithProgressAsync(List<BiometricsLog> logs, IProgress<(int Processed, int Total, string Status)> progress);
    }
}