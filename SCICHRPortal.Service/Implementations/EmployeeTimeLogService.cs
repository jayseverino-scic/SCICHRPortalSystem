using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Implementations;
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

        public Task<EmployeeTimeLogEmployeeLookupPage> SearchEmployeesAsync(string term, int page) =>
            EmployeeTimeLogRepository.SearchEmployeesAsync(term, page);

        public Task<EmployeeTimeLogPage> GetPageAsync(EmployeeTimeLogPageQuery query, CancellationToken cancellationToken = default) =>
            EmployeeTimeLogRepository.GetPageAsync(query, cancellationToken);

        public async Task<bool> DeleteAsync(int id)
        {
            return await EmployeeTimeLogRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAsync(EmployeeTimeLog employeeTimeLog)
        {
            return await EmployeeTimeLogRepository.UpdateAsync(employeeTimeLog);
        }

        public async Task<Tuple<IEnumerable<EmployeeTimeLog>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            return await EmployeeTimeLogRepository.FilterAsync(pageNumber, pageSize, searchKeyword, startDate,endDate, deviceName);
        }

        //public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByDeptAsync(int departmentId, DateTime logDate)
        //{
        //    return await EmployeeTimeLogRepository.GetDailyLogByDeptAsync(departmentId, logDate);
        //}
        public async Task<IEnumerable<EmployeeTimeLog>> GetDailyLogByProjectAsync(int projectId, DateTime logDate)
        {
            return await EmployeeTimeLogRepository.GetDailyLogByProjectAsync(projectId, logDate);
        }
        public async Task<DuplicateMessage> HasDuplicateName(EmployeeTimeLog employeeTimeLog)
        {
            return await EmployeeTimeLogRepository.HasDuplicateName(employeeTimeLog);
        }

        public async Task InsertAsync(EmployeeTimeLog entity)
        {
            await EmployeeTimeLogRepository.InsertAsync(entity);
        }

        public Task<(int Imported, int Skipped)> ImportAsync(IReadOnlyList<EmployeeTimeLog> rows, string actor, ImportAudit? audit = null)
        {
            var now = DateTime.UtcNow;
            foreach (var row in rows)
            {
                row.CreatedAt = now;
                row.CreatedBy = actor;
                row.Comment = TimeLogChanges.Append(null, actor, [$"Imported time record ({row.SystemRemarks})"], now);
            }
            return EmployeeTimeLogRepository.ImportAsync(rows, audit);
        }

        public async Task<EmployeeTimeLog> GetAsync(int id)
        {
            return await EmployeeTimeLogRepository.GetAsync(id);
        }

        public async Task<IEnumerable<EmployeeTimeLog>> GetAllAsync()
        {
            return await EmployeeTimeLogRepository.GetAllAsync();
        }
        public async Task<IEnumerable<EmployeeTimeLog>> FilterByProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            return await EmployeeTimeLogRepository.FilterByProjectAndDateRange(startDate, endDate, projectName);
        }
    }
}
