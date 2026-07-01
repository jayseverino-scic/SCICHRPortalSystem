using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
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

    }
}
