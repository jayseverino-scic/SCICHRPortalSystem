using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;
using SCICHRPortal.Data.Entities;
using System.Linq.Expressions;

namespace SCICHRPortal.Repository.Interfaces;

public interface IUserManagementRepository : IScopedService
{
    Task<UserPage> GetPageAsync(UserPageQuery query, CancellationToken cancellationToken);
    Task<UserRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserRoleOption>> GetRolesAsync(CancellationToken cancellationToken);
    Task<User?> GetAccountAsync(int id, CancellationToken cancellationToken);
    Task<User?> GetAccountByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<bool> AnyAccountAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    Task ExecuteWriteAsync(Func<Task> operation, CancellationToken cancellationToken);
    Task SaveAccountAsync(User account, string actor, CancellationToken cancellationToken);
}
