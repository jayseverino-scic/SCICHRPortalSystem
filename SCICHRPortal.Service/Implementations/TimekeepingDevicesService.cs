using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class TimekeepingDevicesService : ITimekeepingDevicesService
    {
        private ITimekeepingDevicesRepository TimekeepingDevicesRepository { get; }

        public TimekeepingDevicesService(ITimekeepingDevicesRepository timekeepingDevicesRepository)
        {
            TimekeepingDevicesRepository = timekeepingDevicesRepository;
        }

        public Task<IEnumerable<TimekeepingDevices>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<TimekeepingDevices> GetDuplicateAsync(TimekeepingDevices timekeepingDevices)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(TimekeepingDevices entity)
        {
            await TimekeepingDevicesRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(TimekeepingDevices entity)
        {
            return await TimekeepingDevicesRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<TimekeepingDevices>> GetAllAsync()
        {
            return await TimekeepingDevicesRepository.GetAllAsync();
        }

        public async Task<TimekeepingDevices> GetAsync(int id)
        {
            return await TimekeepingDevicesRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await TimekeepingDevicesRepository.DeleteAsync(id);
        }

        public async Task<Tuple<IEnumerable<TimekeepingDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await TimekeepingDevicesRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(TimekeepingDevices timekeepingDevices)
        {
            return await TimekeepingDevicesRepository.HasDuplicateName(timekeepingDevices);
        }
    }
}
