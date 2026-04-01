using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class UserService : IUserService
    {
        public IUserRepository UserRepository { get; }

        public UserService(IUserRepository userRepository)
        {
            UserRepository = userRepository;
        }

        public async Task<User> GetByUserNameAsync(string username)
        {
            return await UserRepository.GetByUserNameAsync(username);
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int id)
        {
            return await UserRepository.GetUserRolesAsync(id);
        }

        public async Task SaveChanges()
        {
            await UserRepository.SaveChangesAsync();
        }

        public async Task<User> GetDuplicateAsync(User user)
        {
            return await UserRepository.GetDuplicateAsync(user);
        }

        public async Task<bool> HasDuplicateNameAsync(string name)
        {
            return await UserRepository.HasDuplicateNameAsync(name);
        }

        public async Task InsertAsync(User entity)
        {
            await UserRepository.InsertAsync(entity);
        }

        public async Task<User> GetAsync(int id)
        {
            return await UserRepository.GetAsync(id);
        }

        public async Task ApprovedAsync(int id)
        {
            await UserRepository.ApprovedAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await UserRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<User>> GetUnApprovedUserAsync()
        {
            return await UserRepository.GetUnApprovedUserAsync();
        }

        public async Task<Tuple<IEnumerable<User>, int>> FilterUnApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await UserRepository.FilterUnApprovedUserAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<Tuple<IEnumerable<User>, int>> FilterApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await UserRepository.FilterApprovedUserAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task ResetPassword(User user)
        {
            await UserRepository.ResetPassword(user);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await UserRepository.GetByEmailAsync(email);
        }

        public async Task UpdateAsync(User entity)
        {
            await UserRepository.UpdateAsync(entity);
        }

        public async Task<List<UserRole>> GetByRoleIdsync(int roleId)
        {
            return await UserRepository.GetByRoleIdsync(roleId);
        }

        public async Task<User> GetDuplicateEmailAsync(User user)
        {
            return await UserRepository.GetDuplicateEmailAsync(user);
        }

        public async Task ToggleActiveAsync(bool isActive, int id)
        {
            await UserRepository.ToggleActiveAsync(isActive, id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await UserRepository.GetAllAsync();
        }
    }
}
