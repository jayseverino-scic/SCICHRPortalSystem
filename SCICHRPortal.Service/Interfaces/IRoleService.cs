using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces
{
    public interface IRoleService :
        IScopedService,
        IListRetriever<Role>,
         IRetriever<Role, int>
    {
        Task<IEnumerable<Role>> FilterAsync(string filter);
        Task<bool> HasDuplicateNameAsync(string name);
        Task<Role> GetDuplicateAsync(Role role);
        Task<RoleWriteResult> WriteAsync(UserOperation operation, int roleId, RoleWriteRequest? request, int actorId, string actor, CancellationToken cancellationToken);
        Task<Tuple<IEnumerable<Role>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Role role);
    }
}
