using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IOvertimeRepository : IScopedService
    {
        Task<OvertimePage> GetPageAsync(OvertimePageQuery query, CancellationToken cancellationToken);
    }
}
