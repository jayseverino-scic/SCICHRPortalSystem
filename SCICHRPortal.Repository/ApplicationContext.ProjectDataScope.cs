using Microsoft.EntityFrameworkCore;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.Repository;

public partial class ApplicationContext
{
    private bool ProjectDataRestricted => _projectDataScope.Access.IsRestricted;
    private int? ScopedProjectId => _projectDataScope.Access.ProjectId;
    private string? ScopedProjectName => _projectDataScope.Access.ProjectName;
    private string? ScopedProjectCode => _projectDataScope.Access.ProjectCode;

    private void ConfigureProjectDataScope(ModelBuilder builder)
    {
        builder.Entity<Project>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || row.Id == ScopedProjectId);
        builder.Entity<Employee>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.ProjectId == ScopedProjectId);
        builder.Entity<EmployeeTimeLog>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<EmployeeAttendance>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<EmployeeShift>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId &&
                (row.ProjectId == null || row.ProjectId == ScopedProjectId));
        builder.Entity<EmployeeDailyAttendanceComputation>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.ProjectId == ScopedProjectId &&
                row.Employee != null && !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<EmployeeDailyAttendanceImportCheckpoint>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || row.ProjectId == ScopedProjectId);
        builder.Entity<OfficialBusiness>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<Leave>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<LeaveRequest>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue && row.Employee != null &&
                !row.Employee.Deleted && row.Employee.ProjectId == ScopedProjectId);
        builder.Entity<TimeLogAttachment>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || EmployeeTimeLog.Any(log => log.TimeLogId == row.TimeLogId && !log.Deleted));
        builder.Entity<BiometricsLog>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectName != null && row.ProjectName != null &&
                row.ProjectName.Trim().ToUpper() == ScopedProjectName);
        builder.Entity<Holiday>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectId.HasValue &&
                (!HolidayProject.Any(link => link.HolidayId == row.HolidayId) ||
                 HolidayProject.Any(link => link.HolidayId == row.HolidayId && link.ProjectId == ScopedProjectId)));
        // Audit rows contain changes across projects, without a reliable project key.
        builder.Entity<Audit>().HasQueryFilter("ProjectDataScope", row => !ProjectDataRestricted);

        // Legacy external navigation types also occur in the SCIC EF model.
        builder.Entity<XCompany_Branch>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || ScopedProjectCode != null && row.Code != null && row.Code.Trim().ToUpper() == ScopedProjectCode ||
                ScopedProjectCode == null && ScopedProjectName != null && row.Name != null && row.Name.Trim().ToUpper() == ScopedProjectName);
        builder.Entity<XEmployee>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || !row._Deleted && Set<XCompany_Branch>().Any(branch => !branch._Deleted && branch.Id == row.Company_Branch_Id));
        builder.Entity<EmployeeShiftDevice>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || Set<XEmployee>().Any(employee => employee.Id == row.EmployeeId));
        builder.Entity<SZKDevices>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || BiometricsLog.Any(log => !log.Deleted && log.SZKDevicesId == row.Id));
        builder.Entity<Device>().HasQueryFilter("ProjectDataScope", row =>
            !ProjectDataRestricted || EmployeeShift.Any(shift => !shift.Deleted && EF.Property<int?>(shift, "DeviceId") == row.Id) ||
                BiometricsLog.Any(log => !log.Deleted && log.DeviceName != null &&
                    (log.DeviceName == row.Name || log.DeviceName == row.SerialNumber)));
    }
}
