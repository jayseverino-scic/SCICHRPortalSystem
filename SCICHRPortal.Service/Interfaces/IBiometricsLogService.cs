using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IBiometricsLogService :
        IScopedService,
         IInserter<BiometricsLog>,
         IRetriever<BiometricsLog, int>,
         IListRetriever<BiometricsLog>
    {
        Task<bool> UpdateAsync(BiometricsLog biometricsLog);
        Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<BiometricsLog>> GetDailyLogAsync(DateTime logDate);
    }
}
