using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings;

public sealed class EmployeeDailyAttendanceComputationMap
{
    public EmployeeDailyAttendanceComputationMap(EntityTypeBuilder<EmployeeDailyAttendanceComputation> builder)
    {
        builder.HasKey(row => new { row.ProjectId, row.AttendanceDate, row.TimeLogId });
        builder.HasOne(row => row.Project).WithMany().HasForeignKey(row => row.ProjectId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(row => row.Employee).WithMany().HasForeignKey(row => row.EmployeeId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(row => row.TimeLog).WithMany().HasForeignKey(row => row.TimeLogId).OnDelete(DeleteBehavior.NoAction);
    }
}
