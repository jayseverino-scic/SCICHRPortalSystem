using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class EmployeeShiftMap
    {
        public EmployeeShiftMap(EntityTypeBuilder<EmployeeShift> entityBuilder) 
        {
            entityBuilder.HasKey(e => e.AssignedShiftId);
            entityBuilder.Property(e => e.ShiftId).IsRequired();
            entityBuilder.Property(e => e.EmployeeId).IsRequired();
            entityBuilder.Property(e => e.ShiftDate).IsRequired();

            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);

            entityBuilder.HasOne(e => e.Department)
               .WithMany()
               .HasForeignKey(u => u.DepartmentId)
               .OnDelete(DeleteBehavior.NoAction);

            entityBuilder.HasOne(u => u.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
