using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.Service.Implementations
{
    public sealed class OvertimeService(IOvertimeRepository repository, ProjectDataScope projectDataScope) : IOvertimeService
    {
        public async Task<OvertimePage?> GetPageAsync(OvertimePageQuery query, CancellationToken cancellationToken)
        {
            // The Clara Forms summary has no project identifier for project-scoped reads.
            if (projectDataScope.Access.IsRestricted)
            {
                return null;
            }

            return await repository.GetPageAsync(query, cancellationToken);
        }
    }
}
