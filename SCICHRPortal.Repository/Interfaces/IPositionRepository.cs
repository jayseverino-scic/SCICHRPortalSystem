using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IPositionRepository : IRepository,
        IScopedService,
         IInserter<Position>,
         IRetriever<Position, int>,
         IListRetriever<Position>
    {
        Task<bool> DeleteAsync(int categoryId);
        Task<bool> UpdateAsync(Position position);
        Task<Tuple<IEnumerable<Position>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Position position);
    }
}
