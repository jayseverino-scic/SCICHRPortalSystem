using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Service.Implementations;

public sealed partial class UserManagementService
{
    private async Task<UserWriteResult> ApplyMutationAsync(UserMutation mutation, int actorId, string actor, CancellationToken cancellationToken)
    {
        // ExecuteAsync keeps these fresh reads, domain decisions, and all saves in one serializable transaction.
        var actorAccount = await Repository.GetAccountAsync(actorId, cancellationToken);
        if (!IsAvailable(actorAccount)) return new(UserWriteStatus.Forbidden, "Your account cannot perform this action.");
        if (mutation.Operation is UserOperation.Profile or UserOperation.ChangePassword)
        {
            if (mutation.UserId != actorId) return new(UserWriteStatus.Forbidden, "Only your own profile can be changed.");
        }
        else if (!HasRole(actorAccount, "Administrator")) return new(UserWriteStatus.Forbidden, "Administrator access is required.");

        var creating = mutation.Operation == UserOperation.Create;
        var account = creating ? new User { IsApproved = true, Active = true, CreatedAt = DateTime.UtcNow, CreatedBy = actor, UserRoles = [] }
            : await Repository.GetAccountAsync(mutation.UserId, cancellationToken);
        if (account == null) return new(UserWriteStatus.NotFound, "User was not found.");
        if (mutation.Operation == UserOperation.ChangePassword &&
            (mutation.ExpectedPasswordHash == null || account.Password != mutation.ExpectedPasswordHash))
            return new(UserWriteStatus.Conflict, "Your password changed while saving. Sign in again and retry.");

        var removingAccess = mutation.Operation is UserOperation.Delete or UserOperation.Deactivate;
        if (removingAccess && account.UserId == actorId)
            return new(UserWriteStatus.Conflict, "You cannot delete or deactivate your own account.");

        var roles = await Repository.GetRolesAsync(cancellationToken);
        if (mutation.RoleIds != null && !mutation.RoleIds.All(id => roles.Any(role => role.RoleId == id)))
            return new(UserWriteStatus.Invalid, "A selected role is no longer available. Reload the form.");
        var wasAdministrator = !creating && IsAvailable(account) && HasRole(account, "Administrator");
        var removesAdministratorRole = mutation.RoleIds != null &&
            !roles.Any(role => role.Name == "Administrator" && mutation.RoleIds.Contains(role.RoleId));
        if (wasAdministrator && (removingAccess || removesAdministratorRole) &&
            !await Repository.AnyAccountAsync(user => user.UserId != account.UserId && !user.Deleted && user.Active && user.IsApproved && !user.Locked &&
                user.UserRoles!.Any(link => !link.Deleted && !link.Role!.Deleted && link.Role.Name == "Administrator"), cancellationToken))
            return new(UserWriteStatus.Conflict, "The last active Administrator must retain access.");

        if (mutation.Profile != null && await Repository.AnyAccountAsync(user => !user.Deleted && user.UserId != account.UserId &&
            user.Email!.Trim().ToLower() == mutation.Profile.Email, cancellationToken))
            return new(UserWriteStatus.Conflict, "An account already uses this email address.");
        if (mutation.Operation == UserOperation.Approve)
        {
            if (account.IsApproved) return new(UserWriteStatus.Conflict, "This account is already approved.");
            if (!CurrentRoles(account).Any())
                return new(UserWriteStatus.Invalid, "Assign at least one available role before approving this account.");
        }
        if (mutation.Operation == UserOperation.ResetPassword && (!account.IsApproved || !account.Active))
            return new(UserWriteStatus.Conflict, "Approve and activate this account before resetting its password.");

        if (mutation.Profile != null)
        {
            account.FirstName = mutation.Profile.FirstName;
            account.MiddleName = mutation.Profile.MiddleName;
            account.LastName = mutation.Profile.LastName;
            account.Email = mutation.Profile.Email;
            account.ContactNumber = mutation.Profile.ContactNumber;
        }
        if (mutation.Operation == UserOperation.Approve) account.IsApproved = true;
        if (mutation.Operation == UserOperation.Activate) account.Active = true;
        if (mutation.Operation == UserOperation.Deactivate) account.Active = false;
        if (mutation.Operation == UserOperation.Delete) account.Deleted = true;
        if (mutation.PasswordHash != null)
        {
            account.Password = mutation.PasswordHash; account.Salt = mutation.Salt;
            account.IsPasswordChanged = false; account.Locked = false; account.LoginAttempts = 0;
        }
        if (!creating) { account.UpdatedAt = DateTime.UtcNow; account.UpdatedBy = actor; }
        if (mutation.RoleIds != null || mutation.Operation == UserOperation.Delete)
            ReplaceRoles(account, mutation.Operation == UserOperation.Delete ? [] : mutation.RoleIds!, actor);
        await Repository.SaveAccountAsync(account, actor, cancellationToken);
        return new(UserWriteStatus.Success, UserId: account.UserId, Email: account.Email, DisplayName: $"{account.FirstName} {account.LastName}");
    }

    private static void ReplaceRoles(User account, int[] roleIds, string actor)
    {
        account.UserRoles ??= [];
        var desired = roleIds.ToHashSet();
        foreach (var group in account.UserRoles.GroupBy(link => link.RoleId).ToArray())
        {
            // Restore one historical assignment per role; keep duplicate history deleted.
            var retained = desired.Contains(group.Key) ? group.OrderBy(link => link.UserRoleId).First() : null;
            foreach (var link in group)
            {
                link.Deleted = link != retained;
                link.UpdatedAt = DateTime.UtcNow; link.UpdatedBy = actor;
            }
        }
        foreach (var roleId in desired.Except(account.UserRoles.Select(link => link.RoleId)).ToArray())
            account.UserRoles.Add(new UserRole { RoleId = roleId, CreatedAt = DateTime.UtcNow, CreatedBy = actor });
    }

    // Service-owned operation state; never an HTTP or repository write contract.
    private sealed record UserMutation(UserOperation Operation, int UserId, UserProfileRequest? Profile = null,
        int[]? RoleIds = null, string? PasswordHash = null, string? Salt = null, string? ExpectedPasswordHash = null);
}
