using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces;

public interface IProjectAccessService : IScopedService
{
    Task<ProjectDataAccess> GetAccessAsync(int userId, CancellationToken cancellationToken);
}
