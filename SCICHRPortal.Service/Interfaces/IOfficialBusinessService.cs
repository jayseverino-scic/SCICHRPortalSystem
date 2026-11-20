using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IOfficialBusinessService : IScopedService
    {
        Task<OfficialBusinessPage> GetPageAsync(OfficialBusinessPageQuery query, CancellationToken cancellationToken);
        Task<OfficialBusinessRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
        Task<OfficialBusinessEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken);
        Task<OfficialBusinessWriteResult> CreateAsync(OfficialBusinessWriteRequest request, string actor, CancellationToken cancellationToken = default);
        Task<OfficialBusinessWriteResult> UpdateAsync(int id, OfficialBusinessWriteRequest request, string actor, CancellationToken cancellationToken = default);
        Task<OfficialBusinessWriteResult> DeleteAsync(int id, string actor, CancellationToken cancellationToken = default);
        Task<OfficialBusinessImportResult> ImportAsync(IReadOnlyList<OfficialBusinessImportRow> rows, string actor, CancellationToken cancellationToken = default);
    }
}
