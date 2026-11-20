using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ILeaveService : IScopedService
    {
        Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken);
        Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
        Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken);
        Task<List<LeaveTypeLookupItem>> GetLeaveTypesAsync(CancellationToken cancellationToken);
        Task<LeaveWriteResult> CreateAsync(LeaveWriteRequest request, string actor, CancellationToken cancellationToken = default);
        Task<LeaveWriteResult> UpdateAsync(int id, LeaveWriteRequest request, string actor, CancellationToken cancellationToken = default);
        Task<LeaveWriteResult> DeleteAsync(int id, string actor, CancellationToken cancellationToken = default);
        Task<LeaveImportResult> ImportAsync(IReadOnlyList<LeaveImportRow> rows, string actor, CancellationToken cancellationToken = default);
    }
}
