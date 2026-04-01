using SCICHRPortal.Core.Interfaces;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Repository.Interfaces
{
    public interface IUserRepository: IRepository,
        IScopedService,
         IInserter<User>,
         IRetriever<User, int>,
         IUpdater<User>,
        IListRetriever<User>
    {
        Task<User> GetByUserNameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<List<UserRole>> GetByRoleIdsync(int roleId);
        Task<IEnumerable<UserRole>> GetUserRolesAsync(int id);
        Task<User> GetDuplicateAsync(User user);
        Task<User> GetDuplicateEmailAsync(User user);
        Task<bool> HasDuplicateNameAsync(string name); 
        Task ApprovedAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task ToggleActiveAsync(bool isActive, int id);
        Task<IEnumerable<User>> GetUnApprovedUserAsync();
        Task ResetPassword(User user);
        Task<Tuple<IEnumerable<User>, int>> FilterUnApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword);
        Task<Tuple<IEnumerable<User>, int>> FilterApprovedUserAsync(int pageNumber, int pageSize, string searchKeyword);

    }
}
