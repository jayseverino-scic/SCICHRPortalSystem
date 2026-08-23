using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class ProjectService : IProjectService
    {
        private IProjectRepository ProjectRepository { get; }

        public ProjectService(IProjectRepository projectRepository)
        {
            ProjectRepository = projectRepository;
        }

        public Task<IEnumerable<Project>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Project> GetDuplicateAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Project entity)
        {
            await ProjectRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Project entity)
        {
            return await ProjectRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await ProjectRepository.GetAllAsync();
        }

        public async Task<Project> GetAsync(int id)
        {
            return await ProjectRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await ProjectRepository.DeleteAsync(id);
        }

        public async Task<Tuple<IEnumerable<Project>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await ProjectRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Project project)
        {
            return await ProjectRepository.HasDuplicateName(project);
        }
        public async Task<Project> GetProjectCodeAsync(string projectCode)
        {
            return await ProjectRepository.GetProjectCodeAsync(projectCode);
        }
        public async Task<Project> GetProjectNameAsync(string projectName)
        {
            return await ProjectRepository.GetProjectNameAsync(projectName);
        }
    }
}
