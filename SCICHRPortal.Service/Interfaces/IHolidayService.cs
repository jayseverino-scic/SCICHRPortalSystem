using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IHolidayService :
        IScopedService,
         IRetriever<Holiday, int>,
         IListRetriever<Holiday>
    {
        Task<bool> DeleteAsync(int holidayId);
        Task<Tuple<IEnumerable<Holiday>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<HolidayWriteResult> CreateAsync(HolidayCreateRequest request);
        Task<HolidayWriteResult> UpdateAsync(HolidayUpdateRequest request);
    }
}
