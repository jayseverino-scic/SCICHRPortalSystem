using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces;

public interface IProjectAccessRepository : IScopedService
{
    Task<IReadOnlyList<EmployeeProjectFact>> GetEmployeeProjectsAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjectIdentifierFact>> GetProjectIdentifiersAsync(CancellationToken cancellationToken);
}
