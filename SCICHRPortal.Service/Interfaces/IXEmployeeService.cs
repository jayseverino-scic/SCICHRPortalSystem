using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IXEmployeeService :
        IScopedService,
         IRetriever<XEmployee, int>,
         IListRetriever<XEmployee>
    {
        Task<Tuple<IEnumerable<XEmployee>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<XEmployee>> GetEmployeeByDepartment(int departmentId);
        Task<IEnumerable<XEmployee>> GetEmployeeByProject(int projectId);
        Task<XEmployee> GetByEmployeeNoAsync(string employeeNo);
    }
}
