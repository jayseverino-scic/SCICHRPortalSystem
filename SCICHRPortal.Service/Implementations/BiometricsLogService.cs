using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.TimekeepingTables;
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

        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            return await BiometricsLogRepository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate,endDate, deviceName);
        }

        public async Task<Tuple<IEnumerable<BiometricsLog>, int>> FilterPerProjectAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? projectName)
        {
            return await BiometricsLogRepository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate, endDate, projectName);
        }
        public async Task<IEnumerable<BiometricsLog>> FilterByDateRange(DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            return await BiometricsLogRepository.FilterByDateRange(startDate, endDate, deviceName);
        }
        public async Task<IEnumerable<BiometricsLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            return await BiometricsLogRepository.FilterByProjectAndDateRange(startDate, endDate, projectName);
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
        public async Task<IEnumerable<STimeLogs>> ImportDbDateRange(DateTime? startDate, DateTime? endDate, string? serialNumber)
        {
            return await BiometricsLogRepository.ImportDbDateRange(startDate, endDate, serialNumber);
        }
    }
}
