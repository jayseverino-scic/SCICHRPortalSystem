using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings;

public sealed class EmployeeDailyAttendanceImportCheckpointMap
{
    public EmployeeDailyAttendanceImportCheckpointMap(EntityTypeBuilder<EmployeeDailyAttendanceImportCheckpoint> builder)
    {
        builder.HasKey(row => new { row.ProjectId, row.AttendanceDate });
        builder.HasOne(row => row.Project).WithMany().HasForeignKey(row => row.ProjectId).OnDelete(DeleteBehavior.NoAction);
    }
}
