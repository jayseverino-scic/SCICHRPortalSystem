using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IHolidayRepository : IRepository,
        IScopedService,
         IRetriever<Holiday, int>,
         IListRetriever<Holiday>
    {
        Task<bool> DeleteAsync(int holidayId);
        Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task InsertAsync(Holiday holiday, IReadOnlyCollection<int> projectIds);
        Task UpdateAsync(Holiday holiday, IReadOnlyCollection<int> projectIds);
    }
}
