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
            //entityBuilder.Ignore(e => e.Company);
            //entityBuilder.Property(e => e.Company_Branch_Id).HasField("Company_Branch_Id");
            //entityBuilder.Ignore(e => e.Department);
            //entityBuilder.Property(e => e.DepartmentId).HasField("DepartmentId");
            //entityBuilder.Ignore(e => e.Shift);
            //entityBuilder.Property(e => e.ShiftId).HasField("ShiftId");
            //entityBuilder.Ignore(e => e.Employee);
            //entityBuilder.Property(e => e.EmployeeId).HasField("EmployeeId");
            entityBuilder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.NoAction);

            //entityBuilder.HasOne(e => e.Department)
            //   .WithMany()
            //   .HasForeignKey(u => u.DepartmentId)
            //   .OnDelete(DeleteBehavior.NoAction);

            //entityBuilder.HasOne(e => e.Shift)
            //   .WithMany()
            //   .HasForeignKey(u => u.ShiftId)
            //   .OnDelete(DeleteBehavior.NoAction);
            //entityBuilder.HasOne(e => e.Company)
            //   .WithMany()
            //   .HasForeignKey(u => u.Company_Branch_Id)
            //   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
