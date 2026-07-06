using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ISGroupsRepository : IRepository,
        IScopedService,
         IRetriever<SGroups, Guid>,
         IListRetriever<SGroups>
    {
        Task<Tuple<IEnumerable<SGroups>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    }
}
