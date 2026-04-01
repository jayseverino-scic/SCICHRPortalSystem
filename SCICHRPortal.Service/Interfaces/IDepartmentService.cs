using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IDepartmentService :
        IScopedService,
         IInserter<Department>,
         IRetriever<Department, int>,
         IListRetriever<Department>
    {
        Task<bool> DeleteAsync(int departmentId);
        Task<bool> UpdateAsync(Department department);
        Task<Tuple<IEnumerable<Department>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Department department);
    }
}
