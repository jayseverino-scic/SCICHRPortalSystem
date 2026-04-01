using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IEmployeeTimeLogService :
        IScopedService,
         IInserter<EmployeeTimeLog>,
         IRetriever<EmployeeTimeLog, int>,
         IListRetriever<EmployeeTimeLog>
    {
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(EmployeeTimeLog employeeTimeLog);
        Task<Tuple<IEnumerable<EmployeeTimeLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByDeptAsync(int departmentId, DateTime logDate);
        Task<DuplicateMessage> HasDuplicateName(EmployeeTimeLog employeeTimeLog);
    }
}
