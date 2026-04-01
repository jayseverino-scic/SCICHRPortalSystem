using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IHolidayRepository : IRepository,
        IScopedService,
         IInserter<Holiday>,
         IRetriever<Holiday, int>,
         IListRetriever<Holiday>
    {
        Task<bool> DeleteAsync(int holidayId);
        Task<bool> UpdateAsync(Holiday holiday);
        Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Holiday holiday);
    }
}
