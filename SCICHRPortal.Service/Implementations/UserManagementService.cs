using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Cryptography;

namespace SCICHRPortal.Service.Implementations;

public sealed partial class UserManagementService : IUserManagementService
{
    private IUserManagementRepository Repository { get; }
    private IUserCredentialDelivery Delivery { get; }
    private ILogger<UserManagementService> Logger { get; }
    private IEmployeeService Employees { get; }
    public UserManagementService(IUserManagementRepository repository, IUserCredentialDelivery delivery,
        ILogger<UserManagementService> logger, IEmployeeService employees)
    {
        Repository = repository;
        Delivery = delivery;
        Logger = logger;
        Employees = employees;
    }

    public Task<UserPage> GetPageAsync(UserPageQuery query, CancellationToken cancellationToken) =>
        Repository.GetPageAsync(query, cancellationToken);
    public Task<UserRow?> GetDetailsAsync(int id, CancellationToken cancellationToken) => Repository.GetDetailsAsync(id, cancellationToken);
    public Task<IReadOnlyList<UserRoleOption>> GetRolesAsync(CancellationToken cancellationToken) => Repository.GetRolesAsync(cancellationToken);

    public async Task<bool> CanManageUsersAsync(int actorId, CancellationToken cancellationToken)
    {
        var account = actorId > 0 ? await Repository.GetAccountAsync(actorId, cancellationToken) : null;
        return IsAvailable(account) && HasRole(account, "Administrator");
    }

    public async Task<UserAuthenticatedProfile?> GetAuthenticatedAsync(int actorId, CancellationToken cancellationToken)
    {
        var account = actorId > 0 ? await Repository.GetAccountAsync(actorId, cancellationToken) : null;
        if (!IsAvailable(account)) return null;
        var roles = CurrentRoles(account).Select(role => role.Name!).ToArray();
        return new(account.UserId, account.FirstName, account.MiddleName, account.LastName,
            $"{account.LastName}, {account.FirstName}", account.Username, account.Email, account.ContactNumber,
            roles.FirstOrDefault() ?? "", roles);
    }

    public Task<UserWriteResult> ChangePasswordAsync(int actorId, string? currentPassword, string? password, string actor, CancellationToken cancellationToken) =>
        ChangeOwnPasswordAsync(actorId, actorId, currentPassword, password, actor, cancellationToken);

    public async Task<UserWriteResult> ChangeOwnPasswordAsync(int actorId, int requestedUserId, string? currentPassword, string? password, string actor, CancellationToken cancellationToken)
    {
        if (actorId <= 0 || (requestedUserId != 0 && requestedUserId != actorId))
            return new(UserWriteStatus.Forbidden, "Only your own password can be changed here.");
        if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length > 255 || password == null || password.Length is < 8 or > 255 ||
            !System.Text.RegularExpressions.Regex.IsMatch(password, "^(?=.*\\d)(?=.*[a-zA-Z]).+$"))
            return new(UserWriteStatus.Invalid, "Enter your current password and a new password of 8–255 characters containing a letter and a number.");
        var account = await Repository.GetAccountAsync(actorId, cancellationToken);
        if (!IsAvailable(account)) return new(UserWriteStatus.Forbidden, "Your account cannot change its password.");
        if (!Hash.Validate(currentPassword, account.Salt!, account.Password!))
            return new(UserWriteStatus.Conflict, "The current password is incorrect.");
        var salt = Salt.Create();
        return await ExecuteAsync(() => ApplyMutationAsync(new UserMutation(UserOperation.ChangePassword, actorId,
            PasswordHash: Hash.Create(password, salt), Salt: salt, ExpectedPasswordHash: account.Password), actorId, actor, cancellationToken), cancellationToken);
    }

    public async Task<UserWriteResult> WriteAsync(UserOperation operation, int id, UserProfileRequest? request,
        int actorId, string actor, CancellationToken cancellationToken)
    {
        if (actorId <= 0) return new(UserWriteStatus.Forbidden, "Sign in to continue.");
        if (!Enum.IsDefined(operation) || operation == UserOperation.ChangePassword || (operation != UserOperation.Create && id <= 0))
            return new(UserWriteStatus.Invalid, "Select a valid user.");
        if (operation == UserOperation.Profile && (id != actorId || request is UserWriteRequest))
            return new(UserWriteStatus.Forbidden, "Only your own profile can be changed here.");

        UserProfileRequest? normalized = null;
        int[]? roleIds = null;
        if (operation is UserOperation.Create or UserOperation.Update or UserOperation.Profile)
        {
            if (request == null) return new(UserWriteStatus.Invalid, "User details are required.");
            normalized = NormalizeProfile(request);
            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(normalized, new ValidationContext(normalized), errors, true))
                return new(UserWriteStatus.Invalid, string.Join(" ", errors.Select(error => error.ErrorMessage)));
            if (operation != UserOperation.Profile)
            {
                roleIds = (request as UserWriteRequest)?.RoleIds?.Distinct().ToArray();
                if (roleIds == null || roleIds.Length == 0 || roleIds.Length > 100 || roleIds.Any(roleId => roleId <= 0))
                    return new(UserWriteStatus.Invalid, "Select at least one valid role (at most 100).");
            }
        }

        var needsPassword = operation is UserOperation.Create or UserOperation.Approve or UserOperation.ResetPassword;
        var password = needsPassword ? GeneratePassword() : null;
        var salt = needsPassword ? Salt.Create() : null;
        var mutation = new UserMutation(operation, id, normalized, roleIds,
            password == null ? null : Hash.Create(password, salt!), salt);
        var result = await ExecuteAsync(() => ApplyMutationAsync(mutation, actorId, actor, cancellationToken), cancellationToken);
        if (result.Status != UserWriteStatus.Success || password == null) return result;
        try
        {
            await Delivery.SendAsync(result.Email!, result.DisplayName ?? "User", password, cancellationToken);
            return result with { NotificationSent = true };
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "User {UserId} was saved but password delivery failed.", result.UserId);
            return result with { NotificationSent = false,
                Message = "The account was saved, but the password email could not be delivered. Use Reset password to retry delivery." };
        }
    }

    private static UserProfileRequest NormalizeProfile(UserProfileRequest request) => new()
    {
        FirstName = request.FirstName?.Trim(), MiddleName = NullIfEmpty(request.MiddleName),
        LastName = request.LastName?.Trim(), Email = request.Email?.Trim().ToLowerInvariant(),
        ContactNumber = NullIfEmpty(request.ContactNumber)
    };
    private static string GeneratePassword() => "S" + RandomNumberGenerator.GetHexString(20) + "9";
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool IsAvailable([NotNullWhen(true)] User? account) => account is { Deleted: false, Active: true, IsApproved: true, Locked: false };
    private static IEnumerable<Role> CurrentRoles(User account) => (account.UserRoles ?? [])
        .Where(link => !link.Deleted && link.Role is { Deleted: false }).Select(link => link.Role!);
    private static bool HasRole(User account, string name) => CurrentRoles(account).Any(role => role.Name == name);

    private async Task<UserWriteResult> ExecuteAsync(Func<Task<UserWriteResult>> operation, CancellationToken cancellationToken)
    {
        UserWriteResult? result = null;
        try
        {
            await Repository.ExecuteWriteAsync(async () => { result = await operation(); }, cancellationToken);
            return result!;
        }
        catch (UserPersistenceConflictException)
        {
            return new(UserWriteStatus.Conflict, "An email address is already in use or the accounts changed while saving. Reload and try again.");
        }
    }
}
