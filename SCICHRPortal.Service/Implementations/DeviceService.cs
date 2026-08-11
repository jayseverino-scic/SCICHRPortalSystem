using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class DeviceService : IDeviceService
    {
        private IDeviceRepository DeviceRepository { get; }

        public DeviceService(IDeviceRepository deviceRepository)
        {
            DeviceRepository = deviceRepository;
        }

        public Task<IEnumerable<Device>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Device> GetDuplicateAsync(Device device)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Device entity)
        {
            await DeviceRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Device entity)
        {
            return await DeviceRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            return await DeviceRepository.GetAllAsync();
        }

        public async Task<Device> GetAsync(int id)
        {
            return await DeviceRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await DeviceRepository.DeleteAsync(id);
        }

        public async Task<Tuple<IEnumerable<Device>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await DeviceRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Device device)
        {
            return await DeviceRepository.HasDuplicateName(device);
        }
    }
}
