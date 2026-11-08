using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface ITardinessService : IScopedService
    {
        Task<TardinessPage> GetPageAsync(TardinessPageQuery query, CancellationToken cancellationToken = default);
        Task<TardinessExport> GetExportAsync(TardinessFilterQuery query, CancellationToken cancellationToken = default);
    }
}
