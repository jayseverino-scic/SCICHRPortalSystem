using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IOvertimeService : IScopedService
    {
        Task<OvertimePage?> GetPageAsync(OvertimePageQuery query, CancellationToken cancellationToken);
    }
}
