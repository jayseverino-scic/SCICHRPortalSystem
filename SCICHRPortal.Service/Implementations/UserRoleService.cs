using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class UserRoleService : IUserRoleService
    {
        private IUserRoleRepository UserRoleRepository { get; }

        public UserRoleService(IUserRoleRepository userRoleRepository)
        {
            UserRoleRepository = userRoleRepository;
        }

        public async Task InsertAsync(UserRole entity)
        {
           await UserRoleRepository.InsertAsync(entity);
           await UserRoleRepository.SaveChangesAsync();
        }

        public async Task<UserRole> GetByUserIdAsync(int id)
        {
            return await UserRoleRepository.GetByUserIdAsync(id);
        }

        public async Task<List<UserRole>> GetByRoleIdAsync(int roleId)
        {
            return await UserRoleRepository.GetByRoleIdAsync(roleId);
        }

        public async Task<bool> UpdateAsync(UserRole userRole)
        {
            return await UserRoleRepository.UpdateAsync(userRole);
        }

        public async Task<UserRole> GetByUserIdAndRoleIdAsync(int userId, int roleId)
        {
            return await UserRoleRepository.GetByUserIdAndRoleIdAsync(userId, roleId);
        }

        public async Task<UserRole> GetAsync(int id)
        {
            return await UserRoleRepository.GetAsync(id);
        }

        public async Task<List<UserRole>> GetByUserAsync(int id)
        {
            return await UserRoleRepository.GetByUserAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await UserRoleRepository.DeleteAsync(id);
        }

        public async Task<bool> UnDeleteAsync(int id)
        {
            return await UserRoleRepository.UnDeleteAsync(id);
        }
    }
}
