using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings
{
    public class EmployeeTimeLogMap
    {
        public EmployeeTimeLogMap(EntityTypeBuilder<EmployeeTimeLog> entityBuilder) 
        { 
            entityBuilder.HasKey(e => e.TimeLogId);

            entityBuilder.Property(e => e.DateIn).IsRequired();
            entityBuilder.Property(e => e.DateOut).IsRequired();
            entityBuilder.Property(e => e.EmployeeId).IsRequired();
            entityBuilder.Property(e => e.ShiftStart).IsRequired();
            entityBuilder.Property(e => e.ShiftEnd).IsRequired();

            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
