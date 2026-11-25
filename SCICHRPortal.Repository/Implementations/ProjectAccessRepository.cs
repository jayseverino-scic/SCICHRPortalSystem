using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Repository.Interfaces;

namespace SCICHRPortal.Repository.Implementations;

public sealed class ProjectAccessRepository(ApplicationContext context) : IProjectAccessRepository
{
    // Access resolution must read the link before request query restrictions are
    // established. These facts are only returned to the access policy service.
    public async Task<IReadOnlyList<EmployeeProjectFact>> GetEmployeeProjectsAsync(int userId, CancellationToken cancellationToken) =>
        await context.Employee.IgnoreQueryFilters().AsNoTracking()
            .Where(employee => !employee.Deleted && employee.UserId == userId)
            .Select(employee => new EmployeeProjectFact(employee.EmployeeId, employee.ProjectId,
                employee.Project == null ? null : employee.Project.Deleted,
                employee.Project == null ? null : employee.Project.Name,
                employee.Project == null ? null : employee.Project.Code))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProjectIdentifierFact>> GetProjectIdentifiersAsync(CancellationToken cancellationToken) =>
        // Deleted projects can still own retained historical logs and labels.
        await context.Project.IgnoreQueryFilters().AsNoTracking()
            .Select(project => new ProjectIdentifierFact(project.Id, project.Name, project.Code))
            .ToListAsync(cancellationToken);
}
