using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ISPersonnelsRepository : IRepository,
        IScopedService,
         IRetriever<SPersonnels, Guid>,
         IListRetriever<SPersonnels>
    {
        Task<Tuple<IEnumerable<SPersonnels>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<SPersonnels> GetBySPersonnelsNoAsync(string employeeNo);
        Task<List<SPersonnels>> GetByMultiplePersonnelNoAsync(List<string> personnelNumbers);
    }
}
