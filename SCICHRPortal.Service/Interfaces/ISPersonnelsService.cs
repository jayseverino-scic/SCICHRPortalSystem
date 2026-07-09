using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ISPersonnelsService :
        IScopedService,
         IRetriever<SPersonnels, Guid>,
         IListRetriever<SPersonnels>
    {
        Task<Tuple<IEnumerable<SPersonnels>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<SPersonnels> GetBySPersonnelsNoAsync(string employeeNo);
    }
}
