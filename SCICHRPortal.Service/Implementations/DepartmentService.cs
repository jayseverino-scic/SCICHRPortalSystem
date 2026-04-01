using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private IDepartmentRepository DepartmentRepository { get; }

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            DepartmentRepository = departmentRepository;
        }

        public Task<IEnumerable<Department>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Department> GetDuplicateAsync(Department department)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Department entity)
        {
            await DepartmentRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Department entity)
        {
            return await DepartmentRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await DepartmentRepository.GetAllAsync();
        }

        public async Task<Department> GetAsync(int id)
        {
            return await DepartmentRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int shiftId)
        {
            return await DepartmentRepository.DeleteAsync(shiftId);
        }

        public async Task<Tuple<IEnumerable<Department>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await DepartmentRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<DuplicateMessage> HasDuplicateName(Department department)
        {
            return await DepartmentRepository.HasDuplicateName(department);
        }
    }
}
