using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IRoleRepository : IRepository,
        IScopedService,
        IInserter<Role>,
        IListRetriever<Role>,
        IRetriever<Role, int>
    {
        Task<IEnumerable<Role>> FilterAsync(string filter);
        Task<Role> GetDuplicateAsync(Role role);
        Task<bool> HasDuplicateNameAsync(string name);
        Task<bool> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int roleId);
        Task<Tuple<IEnumerable<Role>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<DuplicateMessage> HasDuplicateName(Role role);
    }
}
