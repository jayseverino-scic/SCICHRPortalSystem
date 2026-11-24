using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IOTRequestRepository : IRepository, IScopedService
    {
        Task<LeavePage> GetPageAsync(LeavePageQuery query, CancellationToken cancellationToken);
        Task<LeaveRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
        Task<LeaveEmployeeLookupPage> SearchEmployeesAsync(string term, int page, CancellationToken cancellationToken);
        Task<List<LeaveImportEmployee>> GetImportEmployeesAsync(IReadOnlyCollection<string> normalizedEmployeeNumbers, CancellationToken cancellationToken);
        Task<LeaveWriteResult> ExecuteWriteAsync(Func<Task<LeaveWriteResult>> operation, CancellationToken cancellationToken);
        Task<OTRequest?> GetAsync(int id, CancellationToken cancellationToken);
        Task<bool> EmployeeExistsAsync(int id, CancellationToken cancellationToken);
        Task<bool> HasOverlapAsync(int employeeId, DateTime from, DateTime to, int? excludingId, CancellationToken cancellationToken);
        Task InsertAsync(OTRequest record, CancellationToken cancellationToken);
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
