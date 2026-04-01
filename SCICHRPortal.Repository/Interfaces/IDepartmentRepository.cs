using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IDepartmentRepository : IRepository,
        IScopedService,
         IInserter<Department>,
         IRetriever<Department, int>,
         IListRetriever<Department>
    {
        Task<bool> DeleteAsync(int departmentId);
        Task<bool> UpdateAsync(Department entity);
        Task<Tuple<IEnumerable<Department>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Department department);
    }
}
