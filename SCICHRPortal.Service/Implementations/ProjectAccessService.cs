using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations;

public sealed class ProjectAccessService(IUserManagementRepository users, IProjectAccessRepository projects) : IProjectAccessService
{
    public async Task<ProjectDataAccess> GetAccessAsync(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0) return new(true);
        var account = await users.GetAccountAsync(userId, cancellationToken);
        if (account is not { Deleted: false, Active: true, IsApproved: true, Locked: false }) return new(true);
        if (account.UserRoles?.Any(link => !link.Deleted && link.Role is { Deleted: false, Name: "Administrator" }) == true)
            return new(false);

        var assignments = await projects.GetEmployeeProjectsAsync(userId, cancellationToken);
        // Multiple employee links are ambiguous, even when they currently agree.
        if (assignments.Count != 1) return new(true);
        var assignment = assignments[0];
        if (assignment.ProjectId is not > 0 || assignment.ProjectDeleted != false) return new(true);

        var identifiers = await projects.GetProjectIdentifiersAsync(cancellationToken);
        var name = Normalize(assignment.ProjectName);
        var code = Normalize(assignment.ProjectCode);
        // Text-based external databases must not turn an ambiguous local label
        // into permission to read a different project's devices or personnel.
        if (name != null && identifiers.Count(project => Normalize(project.Name) == name) != 1) name = null;
        if (code != null && identifiers.Count(project => Normalize(project.Code) == code) != 1) code = null;
        return new(true, assignment.ProjectId, name, code);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
