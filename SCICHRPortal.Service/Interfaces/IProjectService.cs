using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IProjectService :
        IScopedService,
         IInserter<Project>,
         IRetriever<Project, int>,
         IListRetriever<Project>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(Project project);
        Task<Tuple<IEnumerable<Project>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Project project);
    }
}
