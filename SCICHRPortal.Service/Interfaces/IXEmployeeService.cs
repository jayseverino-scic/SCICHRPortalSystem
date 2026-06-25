using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IXEmployeeService :
        IScopedService,
         IRetriever<Employee, int>,
         IListRetriever<Employee>
    {
        Task<Tuple<IEnumerable<Employee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<Employee>> GetEmployeeByDepartment(int departmentId);
        Task<Employee> GetByEmployeeNoAsync(string employeeNo);
    }
}
