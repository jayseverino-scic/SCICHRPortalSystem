using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IShiftService :
        IScopedService,
         IInserter<Shift>,
         IRetriever<Shift, int>,
         IListRetriever<Shift>
    {
        Task<bool> DeleteAsync(int shiftId);
        Task<bool> UpdateAsync(Shift shift);
        Task<Tuple<IEnumerable<Shift>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Shift shift);
    }
}
