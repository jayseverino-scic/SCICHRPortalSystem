using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ILeaveRepository : IRepository, IScopedService
    {
        Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken);
        Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
        Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken);
        Task<List<LeaveImportEmployee>> GetImportEmployeesAsync(IReadOnlyCollection<string> normalizedEmployeeNumbers, CancellationToken cancellationToken);
        Task<List<LeaveTypeLookupItem>> GetLeaveTypesAsync(CancellationToken cancellationToken);
        Task<List<LeaveTypeLookupItem>> GetImportLeaveTypesAsync(IReadOnlyCollection<string> normalizedDescriptions, CancellationToken cancellationToken);
        Task<bool> LeaveTypeExistsAsync(int id, CancellationToken cancellationToken);
        Task<TResult> ExecuteWriteAsync<TResult>(
            Func<Task<(TResult Result, bool Commit)>> operation, CancellationToken cancellationToken);
        Task<Leave?> GetAsync(int id, CancellationToken cancellationToken);
        Task<bool> EmployeeExistsAsync(int id, CancellationToken cancellationToken);
        Task<bool> HasOverlapAsync(int employeeId, DateTime from, DateTime to, int? excludingId, CancellationToken cancellationToken);
        Task InsertAsync(Leave record, CancellationToken cancellationToken);
        Task UpdateAsync(Leave record, Leave changes, CancellationToken cancellationToken);
        Task DeleteAsync(Leave record, Leave changes, CancellationToken cancellationToken);
    }
}
