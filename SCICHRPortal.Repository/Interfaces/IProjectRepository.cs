using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IProjectRepository : IRepository,
        IScopedService,
         IInserter<Project>,
         IRetriever<Project, int>,
         IListRetriever<Project>
    {
        Task<bool> DeleteAsync(int projectId);
        Task<bool> UpdateAsync(Project entity);
        Task<Tuple<IEnumerable<Project>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Project project);
        Task<Project> GetProjectCodeAsync(string  projectCode);
        Task<Project> GetProjectNameAsync(string projectName);
    }
}
