using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ISZKDevicesService :
        IScopedService,
         IRetriever<SZKDevices, Guid>,
         IListRetriever<SZKDevices>
    {
        Task<Tuple<IEnumerable<SZKDevices>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    }
}
