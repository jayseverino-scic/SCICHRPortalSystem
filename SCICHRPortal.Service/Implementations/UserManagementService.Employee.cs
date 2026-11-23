using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Utility.Cryptography;

namespace SCICHRPortal.Service.Implementations;

public sealed partial class UserManagementService
{
    public async Task<EmployeeAccountResult> CreateEmployeeAsync(Employee request, int actorId, string actor, CancellationToken cancellationToken)
    {
        Employee? employee = null;
        var result = await ExecuteAsync(async () =>
        {
            if (!await CanManageUsersAsync(actorId, cancellationToken))
                return new(UserWriteStatus.Forbidden, "Administrator access is required.");
            var employeeRoles = (await Repository.GetRolesAsync(cancellationToken)).Where(role => role.Name == "Other").ToArray();
            if (employeeRoles.Length != 1)
                return new(UserWriteStatus.Conflict, "The Other account role is unavailable or ambiguous. Restore the role before creating employees.");
            var profile = NormalizeProfile(new UserProfileRequest { FirstName = request.FirstName, MiddleName = request.MiddleName,
                LastName = request.LastName, Email = request.Email, ContactNumber = request.ContactNumber });
            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(profile, new ValidationContext(profile), errors, true))
                return new(UserWriteStatus.Invalid, string.Join(" ", errors.Select(error => error.ErrorMessage)));
            if (string.IsNullOrWhiteSpace(request.EmployeeNo) || request.EmployeeNo.Length > 20 ||
                profile.LastName!.Length > 30 || (request.Suffix?.Length ?? 0) > 20 || (request.Address?.Length ?? 0) > 2000)
                return new(UserWriteStatus.Invalid, "Enter an employee number (20 characters), last name (30 characters), suffix (20 characters), and address (2000 characters) within the supported limits.");
            employee = new Employee { EmployeeNo = request.EmployeeNo, FirstName = profile.FirstName, MiddleName = profile.MiddleName,
                LastName = profile.LastName, Email = profile.Email, ContactNumber = profile.ContactNumber ?? "", Suffix = request.Suffix ?? "",
                Address = request.Address, DepartmentId = request.DepartmentId, ProjectId = request.ProjectId,
                CreatedAt = DateTime.UtcNow, CreatedBy = actor };
            var duplicate = await Employees.HasDuplicateName(employee);
            if (duplicate.IsDuplicated) return new(UserWriteStatus.Conflict, duplicate.Message);
            var salt = Salt.Create();
            var account = await ApplyMutationAsync(new UserMutation(UserOperation.Create, 0, profile, [employeeRoles[0].RoleId],
                Hash.Create(GeneratePassword(), salt), salt), actorId, actor, cancellationToken);
            if (account.Status != UserWriteStatus.Success) return account;
            employee.UserId = account.UserId;
            // Both repositories share the scoped ApplicationContext and its enclosing transaction.
            await Employees.InsertWithAuditAsync(employee, actor, cancellationToken);
            return account;
        }, cancellationToken);
        return new(result.Status, result.Message, result.Status == UserWriteStatus.Success ? employee!.EmployeeId : 0, result.UserId);
    }
}
