using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SCICHRPortal.Data.DTOs;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UserProfileRequest
{
    [Required, MaxLength(60)] public string? FirstName { get; set; }
    [MaxLength(60)] public string? MiddleName { get; set; }
    [Required, MaxLength(60)] public string? LastName { get; set; }
    [Required, EmailAddress, MaxLength(100)] public string? Email { get; set; }
    [MaxLength(60)] public string? ContactNumber { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class UserWriteRequest : UserProfileRequest
{
    [Required, MinLength(1)] public int[]? RoleIds { get; set; }
}

public sealed class UserPageQuery
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 10;
    public string? Search { get; set; }
    public string? Approval { get; set; }
    public string? State { get; set; }
    public bool IsValid() => Draw >= 0 && Start is >= 0 and <= 10000000 &&
        Length is 5 or 10 or 20 or 40 or 80 && (Search?.Length ?? 0) <= 100 &&
        Approval is null or "" or "approved" or "pending" && State is null or "" or "active" or "inactive";
}

public sealed record UserRoleOption(int RoleId, string Name);
public sealed class UserRow
{
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public bool IsApproved { get; set; }
    public bool Active { get; set; }
    public bool Locked { get; set; }
    public List<UserRoleOption> Roles { get; set; } = [];
}
public sealed record UserPage(int Draw, int RecordsTotal, int RecordsFiltered, IReadOnlyList<UserRow> Data);
public enum UserWriteStatus { Success, Invalid, Conflict, NotFound, Forbidden }
public enum UserOperation { Create, Update, Approve, Activate, Deactivate, Delete, ResetPassword, Profile, ChangePassword }
public sealed record UserWriteResult(UserWriteStatus Status, string? Message = null, int UserId = 0,
    string? Email = null, string? DisplayName = null, bool? NotificationSent = null);

public sealed record UserAuthenticatedProfile(int UserId, string? FirstName, string? MiddleName, string? LastName,
    string Name, string? Username, string? Email, string? ContactNumber, string RoleName, string[] Roles);
public sealed record EmployeeAccountResult(UserWriteStatus Status, string? Message = null, int EmployeeId = 0, int UserId = 0);
public sealed class RoleWriteRequest
{
    public int RoleId { get; set; }
    [Required, MaxLength(60)] public string? Name { get; set; }
    [Required, MaxLength(100)] public string? Description { get; set; }
}
public sealed record RoleWriteResult(UserWriteStatus Status, string? Message = null, int RoleId = 0);
public enum UserAuthenticationStatus { Success, Invalid, Unauthorized, Forbidden, Locked, NotFound, DeliveryFailed, Unavailable }
public sealed record UserAuthenticationResult(UserAuthenticationStatus Status, string? Message = null,
    string? JsonWebToken = null, string? RefreshToken = null, bool IsPasswordChanged = false);

// Provider errors cross the repository boundary without provider types or domain decisions.
public sealed class UserPersistenceConflictException(Exception innerException) : Exception("A concurrent persistence conflict occurred.", innerException);
