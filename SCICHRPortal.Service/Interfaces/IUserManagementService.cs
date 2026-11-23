using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces;

public interface IUserManagementService : IScopedService
{
    Task<UserPage> GetPageAsync(UserPageQuery query, CancellationToken cancellationToken);
    Task<UserRow?> GetDetailsAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserRoleOption>> GetRolesAsync(CancellationToken cancellationToken);
    Task<UserWriteResult> WriteAsync(UserOperation operation, int id, UserProfileRequest? request,
        int actorId, string actor, CancellationToken cancellationToken);
    Task<UserWriteResult> ChangePasswordAsync(int actorId, string? currentPassword, string? password, string actor, CancellationToken cancellationToken);
    Task<UserWriteResult> ChangeOwnPasswordAsync(int actorId, int requestedUserId, string? currentPassword, string? password, string actor, CancellationToken cancellationToken);
    Task<bool> CanManageUsersAsync(int actorId, CancellationToken cancellationToken);
    Task<UserAuthenticatedProfile?> GetAuthenticatedAsync(int actorId, CancellationToken cancellationToken);
    Task<EmployeeAccountResult> CreateEmployeeAsync(SCICHRPortal.Data.Entities.Metadatas.Employee request, int actorId, string actor, CancellationToken cancellationToken);
}

public interface IUserCredentialDelivery
{
    Task SendAsync(string email, string name, string password, CancellationToken cancellationToken);
}
