using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class TimekeepingAdminSetupService : ITimekeepingAdminSetupService
    {
        private ITimekeepingAdminSetupRepository TimekeepingAdminSetupRepository { get; }

        public TimekeepingAdminSetupService(ITimekeepingAdminSetupRepository timekeepingAdminSetupRepository)
        {
            TimekeepingAdminSetupRepository = timekeepingAdminSetupRepository;
        }
        public async Task<bool> UpdateAsync(TimekeepingAdminSetup timekeepingAdminSetup)
        {
            return await TimekeepingAdminSetupRepository.UpdateAsync(timekeepingAdminSetup);
        }
        public async Task<TimekeepingAdminSetup> GetAsync(int id)
        {
            return await TimekeepingAdminSetupRepository.GetAsync(id);
        }
        public async Task<TimekeepingAdminSetup> GetFirstOrDefault()
        {
            return await TimekeepingAdminSetupRepository.GetFirstOrDefault();
        }
    }
}
