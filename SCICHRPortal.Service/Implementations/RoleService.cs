using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Extensions;

namespace SCICHRPortal.Service.Implementations
{
    public class RoleService : IRoleService
    {
        private IRoleRepository RoleRepository { get; }
        private IUserManagementService Users { get; }
        public RoleService(IRoleRepository roleRepository, IUserManagementService users)
        {
            RoleRepository = roleRepository;
            Users = users;
        }

        public async Task<RoleWriteResult> WriteAsync(UserOperation operation, int roleId, RoleWriteRequest? request,
            int actorId, string actor, CancellationToken cancellationToken)
        {
            if (operation is not (UserOperation.Create or UserOperation.Update or UserOperation.Delete) ||
                (operation != UserOperation.Create && roleId <= 0))
                return new(UserWriteStatus.Invalid, "Select a valid role.");
            if (operation != UserOperation.Delete && (request == null || string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Description) || request.Name.Trim().Length > 60 || request.Description.Trim().Length > 100))
                return new(UserWriteStatus.Invalid, "Enter a role name (60 characters) and description (100 characters).");
            RoleWriteResult? result = null;
            try
            {
                await RoleRepository.ExecuteWriteAsync(async () =>
                {
                    if (!await Users.CanManageUsersAsync(actorId, cancellationToken))
                    {
                        result = new(UserWriteStatus.Forbidden, "Administrator access is required.");
                        return;
                    }
                    var creating = operation == UserOperation.Create;
                    var role = creating ? new Role { CreatedAt = DateTime.UtcNow, CreatedBy = actor }
                        : await RoleRepository.GetAsync(roleId);
                    if (role == null) { result = new(UserWriteStatus.NotFound, "Role was not found."); return; }
                    if (!creating && (role.RoleId is 1 or 2 || role.Name is "Administrator" or "Other"))
                    { result = new(UserWriteStatus.Conflict, "Built-in account roles cannot be changed or deleted."); return; }
                    if (operation == UserOperation.Delete)
                    {
                        if (await RoleRepository.HasAssignmentsAsync(roleId, cancellationToken))
                        { result = new(UserWriteStatus.Conflict, "This role is in use."); return; }
                        role.Deleted = true;
                    }
                    else
                    {
                        var name = request!.Name!.Trim();
                        var description = request.Description!.Trim();
                        var roles = await RoleRepository.GetAllAsync();
                        if (roles.Any(existing => existing.RoleId != roleId && string.Equals(existing.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                        { result = new(UserWriteStatus.Conflict, "Role name is already in use."); return; }
                        if (creating && roles.Any(existing => NormalizeDescription(existing.Description) == NormalizeDescription(description)))
                        { result = new(UserWriteStatus.Conflict, "Description Duplicated"); return; }
                        role.Name = name; role.Description = description;
                    }
                    if (!creating) { role.UpdatedAt = DateTime.UtcNow; role.UpdatedBy = actor; }
                    await RoleRepository.SaveRoleAsync(role, actor, cancellationToken);
                    result = new(UserWriteStatus.Success, RoleId: role.RoleId);
                }, cancellationToken);
                return result!;
            }
            catch (UserPersistenceConflictException)
            {
                return new(UserWriteStatus.Conflict, "This role changed while saving. Reload and try again.");
            }
        }

        public Task<IEnumerable<Role>> GetAllAsync() => RoleRepository.GetAllAsync();
        public Task<Role> GetAsync(int id) => RoleRepository.GetAsync(id);
        public Task<Tuple<IEnumerable<Role>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword) =>
            RoleRepository.FilterAsync(pageNumber, pageSize, searchKeyword);

        public async Task<DuplicateMessage> HasDuplicateName(Role role)
        {
            var roles = await RoleRepository.GetAllAsync();
            var name = roles.Any(existing => string.Equals(existing.Name?.Trim(), role.Name?.Trim(), StringComparison.OrdinalIgnoreCase));
            var description = roles.Any(existing => NormalizeDescription(existing.Description) == NormalizeDescription(role.Description));
            return new DuplicateMessage { IsDuplicated = name || description, Message = name ? "Name Duplicated" : description ? "Description Duplicated" : null };
        }
        private static string NormalizeDescription(string? value) => (value ?? "").ToLowerInvariant().StringSplitThenJoin();

        public Task<IEnumerable<Role>> FilterAsync(string filter) => throw new NotImplementedException();
        public Task<Role> GetDuplicateAsync(Role role) => throw new NotImplementedException();
        public Task<bool> HasDuplicateNameAsync(string name) => throw new NotImplementedException();
    }
}
