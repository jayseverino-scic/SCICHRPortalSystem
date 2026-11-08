using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface ITardinessRepository : IRepository, IScopedService
    {
        Task<TardinessPage> GetPageAsync(TardinessPageQuery query, CancellationToken cancellationToken = default);
        Task<List<TardinessRow>> GetExportAsync(TardinessFilterQuery query, CancellationToken cancellationToken = default);
    }
}
