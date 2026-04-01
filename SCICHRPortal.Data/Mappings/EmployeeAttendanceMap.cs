using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography.X509Certificates;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings
{
    public class EmployeeAttendanceMap
    {
        public EmployeeAttendanceMap(EntityTypeBuilder<EmployeeAttendance> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.EmployeeAttendanceId);
            entityBuilder.Property(e => e.EmployeeId).IsRequired();
            entityBuilder.Property(e => e.TimeLogId).IsRequired();
            entityBuilder.Property(e => e.TimeIn).IsRequired();
            entityBuilder.Property(e => e.TimeOut).IsRequired();
            entityBuilder.Property(e => e.ShiftStart).IsRequired();
            entityBuilder.Property(e => e.ShiftEnd).IsRequired();

            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);

            entityBuilder.HasOne(e => e.EmployeeTimeLog)
                .WithMany()
                .HasForeignKey(u =>u.TimeLogId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
