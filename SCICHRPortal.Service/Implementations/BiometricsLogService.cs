using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class BiometricsLogService : IBiometricsLogService
    {
        private IBiometricsLogRepository BiometricsLogRepository { get; }

        public BiometricsLogService(IBiometricsLogRepository biometricsLogRepository)
        {
            BiometricsLogRepository = biometricsLogRepository;
        }
        public async Task<bool> UpdateAsync(BiometricsLog biometricsLog)
        {
            return await BiometricsLogRepository.UpdateAsync(biometricsLog);
        }

        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate)
        {
            return await BiometricsLogRepository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate,endDate);
        }

        public async Task<IEnumerable<BiometricsLog>> GetDailyLogAsync(DateTime logDate)
        {
            return await BiometricsLogRepository.GetDailyLogAsync(logDate);
        }

        public async Task InsertAsync(BiometricsLog entity)
        {
            await BiometricsLogRepository.InsertAsync(entity);
        }

        public async Task<BiometricsLog> GetAsync(int id)
        {
            return await BiometricsLogRepository.GetAsync(id);
        }

        public async Task<IEnumerable<BiometricsLog>> GetAllAsync()
        {
            return await BiometricsLogRepository.GetAllAsync();
        }
    }
}
