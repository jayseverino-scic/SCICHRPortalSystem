using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IBiometricsLogRepository : IRepository,
        IScopedService,
         IInserter<BiometricsLog>,
         IRetriever<BiometricsLog, int>,
         IListRetriever<BiometricsLog>
    {
        Task<bool> UpdateAsync(BiometricsLog biometricsLog);

        Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName);
        Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterPerProjectAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? projectName);

        Task<IEnumerable<BiometricsLog>> GetDailyLogAsync(DateTime logDate);
        Task<IEnumerable<BiometricsLog>> FilterByDateRange(DateTime? startDate, DateTime? endDate, string? deviceName);
        Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName);
        Task<IEnumerable<STimeLogs>> ImportDbDateRange(DateTime? startDate, DateTime? endDate, string? serialNumber);
        Task BulkInsertAsync(IEnumerable<BiometricsLog> logs);
        Task BulkInsertWithTransactionAsync(IEnumerable<BiometricsLog> logs);
        Task<int> BulkInsertWithReturnCountAsync(IEnumerable<BiometricsLog> logs);
        Task<BulkImportResult> BulkInsertWithResultAsync(List<BiometricsLog> logs);
        Task BulkInsertBiometricsLogsAsync(List<BiometricsLog> logs);
        Task<BulkImportResult> BulkInsertWithProgressAsync(List<BiometricsLog> logs,IProgress<BulkProgress> progress);
    }
}
