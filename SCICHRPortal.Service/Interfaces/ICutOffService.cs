using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ICutOffService :
        IScopedService,
         IInserter<CutOff>,
         IRetriever<CutOff, int>,
         IListRetriever<CutOff>
    {
        Task<bool> DeleteAsync(int cutOffId);
        Task<bool> UpdateAsync(CutOff cutOff);
        Task<Tuple<IEnumerable<CutOff>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(CutOff cutOff);
    }
}
