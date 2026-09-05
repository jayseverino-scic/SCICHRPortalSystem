using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IBiometricsLogRepository : IRepository,
        IScopedService,
         IInserter<BiometricsLog>,
         IRetriever<BiometricsLog, int>,
         IListRetriever<BiometricsLog>
    {
        // Existing CRUD methods
        Task<IEnumerable<BiometricsLog>> GetAllAsync();
        Task<(IEnumerable<BiometricsLog>, int)> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName);
        Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName);
        Task<BiometricsLog> GetByIdAsync(int id);
        Task InsertAsync(BiometricsLog biometricsLog);
        Task UpdateAsync(BiometricsLog biometricsLog);
        Task DeleteAsync(int id);

        // Time logs from TimekeepingContext - FIXED: Use List<TimeLog> instead of dynamic
        Task<List<STimeLogs>> GetTimeLogsFromTimekeepingAsync(DateTime? startDate, DateTime? endDate, string? projectName);

        // Employees from XscribeContext
        //Task<Employee> GetEmployeeFromDBAsync(string personnelNo);

        // Bulk import methods
        Task<int> BulkInsertAsync(List<BiometricsLog> logs);
        Task<(int Inserted, int Failed, List<string> Errors)> BulkInsertWithResultAsync(List<BiometricsLog> logs);
        Task<int> BulkInsertWithTransactionAsync(List<BiometricsLog> logs);
        Task<int> BulkInsertWithProgressAsync(List<BiometricsLog> logs, IProgress<(int Processed, int Total, string Status)> progress);
        //DataTable BuildDataTableFromLogs(List<BiometricsLog> logs);
        Task<List<Employee>> GetEmployeesFromDBAsync(List<string> personnelNumbers);
    }
}