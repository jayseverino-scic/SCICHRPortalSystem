using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeTimeLogService : IEmployeeTimeLogService
    {
        private IEmployeeTimeLogRepository EmployeeTimeLogRepository { get; }

        public EmployeeTimeLogService(IEmployeeTimeLogRepository employeeTimeLogRepository)
        {
            EmployeeTimeLogRepository = employeeTimeLogRepository;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await EmployeeTimeLogRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAsync(EmployeeTimeLog employeeTimeLog)
        {
            return await EmployeeTimeLogRepository.UpdateAsync(employeeTimeLog);
        }

        public async Task<Tuple<IEnumerable<EmployeeTimeLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate)
        {
            return await EmployeeTimeLogRepository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate,endDate);
        }

        public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByDeptAsync(int departmentId, DateTime logDate)
        {
            return await EmployeeTimeLogRepository.GetDailyLogByDeptAsync(departmentId, logDate);
        }
        public async Task<DuplicateMessage> HasDuplicateName(EmployeeTimeLog employeeTimeLog)
        {
            return await EmployeeTimeLogRepository.HasDuplicateName(employeeTimeLog);
        }

        public async Task InsertAsync(EmployeeTimeLog entity)
        {
            await EmployeeTimeLogRepository.InsertAsync(entity);
        }

        public async Task<EmployeeTimeLog> GetAsync(int id)
        {
            return await EmployeeTimeLogRepository.GetAsync(id);
        }

        public async Task<IEnumerable<EmployeeTimeLog>> GetAllAsync()
        {
            return await EmployeeTimeLogRepository.GetAllAsync();
        }
    }
}
