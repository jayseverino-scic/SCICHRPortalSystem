using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private IEmployeeRepository EmployeeRepository { get; }

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            EmployeeRepository = employeeRepository;
        }

        public Task<IEnumerable<Employee>> FilterAsync(string filter)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> GetDuplicateAsync(Employee Employee)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasDuplicateNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Employee entity)
        {
            await EmployeeRepository.InsertAsync(entity);
        }

        public async Task<bool> UpdateAsync(Employee entity)
        {
            return await EmployeeRepository.UpdateAsync(entity);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await EmployeeRepository.GetAllAsync();
        }

        public async Task<Employee> GetAsync(int id)
        {
            return await EmployeeRepository.GetAsync(id);
        }

        public async Task<bool> DeleteAsync(int EmployeeId)
        {
            return await EmployeeRepository.DeleteAsync(EmployeeId);
        }

        public async Task<Tuple<IEnumerable<Employee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<Employee>> GetEmployeeByDepartment(int departmentId)
        {
            return await EmployeeRepository.GetEmployeeByDepartment(departmentId);
        }
        public async Task<DuplicateMessage> HasDuplicateName(Employee Employee)
        {
            return await EmployeeRepository.HasDuplicateName(Employee);
        }

        public async Task<Employee> GetByUserId(int userId)
        {
            return await EmployeeRepository.GetByUserId(userId);
        }
    }
}
