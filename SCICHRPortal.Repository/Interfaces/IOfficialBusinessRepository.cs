using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IOfficialBusinessRepository : IRepository, IScopedService
    {
        Task<OfficialBusinessPage> GetPageAsync(OfficialBusinessPageQuery query, CancellationToken cancellationToken);
        Task<OfficialBusinessRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
        Task<OfficialBusinessEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken);
        Task<List<OfficialBusinessImportEmployee>> GetImportEmployeesAsync(IReadOnlyCollection<string> normalizedEmployeeNumbers, CancellationToken cancellationToken);
        Task<OfficialBusinessWriteResult> ExecuteWriteAsync(Func<Task<OfficialBusinessWriteResult>> operation, CancellationToken cancellationToken);
        Task<OfficialBusiness?> GetAsync(int id, CancellationToken cancellationToken);
        Task<bool> EmployeeExistsAsync(int id, CancellationToken cancellationToken);
        Task<bool> HasOverlapAsync(int employeeId, DateTime from, DateTime to, int? excludingId, CancellationToken cancellationToken);
        Task InsertAsync(OfficialBusiness record, CancellationToken cancellationToken);
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
