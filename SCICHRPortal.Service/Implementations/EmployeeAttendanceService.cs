using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        public IEmployeeAttendanceRepository EmployeeAttendanceRepository { get; }

        public EmployeeAttendanceService(IEmployeeAttendanceRepository employeeAttendanceRepository)
        {
            EmployeeAttendanceRepository = employeeAttendanceRepository;
        }


        public async Task<bool> UpdateAsync(EmployeeAttendance entity)
        {
            return await EmployeeAttendanceRepository.UpdateAsync(entity);
        }

        public async Task<Tuple<IEnumerable<EmployeeAttendance>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeAttendanceRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceFilter(int departmentId, DateTime attendanceDate)
        {
            return await EmployeeAttendanceRepository.EmployeeAttendanceFilter(departmentId, attendanceDate);
        }
        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceCutOffFilter(int departmentId, DateTime fromDate, DateTime toDate)
        {
            return await EmployeeAttendanceRepository.EmployeeAttendanceCutOffFilter(departmentId, fromDate, toDate);
        }
        public async Task<EmployeeAttendance> GetAsync(int id)
        {
            return await EmployeeAttendanceRepository.GetAsync(id);
        }
        public async Task<IEnumerable<EmployeeAttendance>> GetAllAsync()
        {
            return await EmployeeAttendanceRepository.GetAllAsync();
        }

        public async Task InsertAsync(EmployeeAttendance entity)
        {
            await EmployeeAttendanceRepository.InsertAsync(entity);
        }
        public async Task RemoveRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.RemoveRangeAsync(employeeAttendances);
        }
        public async Task UpdateRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.UpdateRangeAsync(employeeAttendances);
        }
        public async Task InsertRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.InsertRangeAsync(employeeAttendances);
        }
    }
}
