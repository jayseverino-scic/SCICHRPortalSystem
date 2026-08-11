using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IDeviceService :
        IScopedService,
         IInserter<Device>,
         IRetriever<Device, int>,
         IListRetriever<Device>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(Device device);
        Task<Tuple<IEnumerable<Device>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Device device);
    }
}
