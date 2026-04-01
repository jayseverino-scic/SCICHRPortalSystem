using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IEmployeeRepository : IRepository,
        IScopedService,
         IInserter<Employee>,
         IRetriever<Employee, int>,
         IListRetriever<Employee>
    {
        Task<bool> DeleteAsync(int employeeId);
    Task<bool> UpdateAsync(Employee employee);
    Task<Tuple<IEnumerable<Employee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
    Task<IEnumerable<Employee>> GetEmployeeByDepartment(int departmentId);
    Task<DuplicateMessage> HasDuplicateName(Employee employee);
    Task<Employee> GetByUserId(int userId);
}
}
