using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class SZKDevicesService : ISZKDevicesService
    {
        private ISZKDevicesRepository DevicesRepository { get; }

        public SZKDevicesService(ISZKDevicesRepository devicesRepository)
        {
            DevicesRepository = devicesRepository;
        }

        public Task<IEnumerable<SZKDevices>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SZKDevices>> GetAllAsync()
        {
            return await DevicesRepository.GetAllAsync();
        }

        public async Task<SZKDevices> GetAsync(Guid id)
        {
            return await DevicesRepository.GetAsync(id);
        }

        public async Task<Tuple<IEnumerable<SZKDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await DevicesRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }
    }
}
