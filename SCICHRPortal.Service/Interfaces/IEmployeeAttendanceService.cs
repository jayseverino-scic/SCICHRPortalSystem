using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IEmployeeAttendanceService : IScopedService,
       IInserter<EmployeeAttendance>
    {
        Task<bool> UpdateAsync(EmployeeAttendance entity);
        Task<Tuple<IEnumerable<EmployeeAttendance>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceFilter(int departmentId, DateTime attendanceDate);
        Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceCutOffFilter(int departmentId, DateTime fromDate, DateTime toDate);
        Task<EmployeeAttendance> GetAsync(int id);
        Task<IEnumerable<EmployeeAttendance>> GetAllAsync();
        Task RemoveRangeAsync(List<EmployeeAttendance> employeeAttendances);
        Task UpdateRangeAsync(List<EmployeeAttendance> employeeAttendances);
        Task InsertRangeAsync(List<EmployeeAttendance> employeeAttendances);
    }
}
