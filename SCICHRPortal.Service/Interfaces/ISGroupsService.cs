using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ISGroupsService :
        IScopedService,
         IRetriever<SGroups, Guid>,
         IListRetriever<SGroups>
    {
        Task<Tuple<IEnumerable<SGroups>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    }
}
