using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IUserRoleRepository: IRepository,
        IScopedService,
      IInserter<UserRole>,
        IRetriever<UserRole, int>
    {
        Task<UserRole> GetByUserIdAsync(int id);
        Task<List<UserRole>> GetByUserAsync(int id);
        Task<List<UserRole>> GetByRoleIdAsync(int roleId);
        Task<bool> UpdateAsync(UserRole userRole);
        Task<UserRole> GetByUserIdAndRoleIdAsync(int userId, int roleId);
        Task<bool> DeleteAsync(int id);
        Task<bool> UnDeleteAsync(int id);
    }
}
