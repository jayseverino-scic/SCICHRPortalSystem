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
            //entityBuilder.HasOne(e => e.Employee)
            //   .WithMany()
            //   .HasForeignKey(e => e.EmployeeId)
            //   .HasPrincipalKey(e => e.Id)
            //   .OnDelete(DeleteBehavior.ClientCascade)
            //   .IsRequired(false);
            //entityBuilder.HasOne(e => e.Company)
            //   .WithMany()
            //   .HasForeignKey(e => e.Company_Branch_Id)
            //   .HasPrincipalKey(e => e.Id)
            //   .OnDelete(DeleteBehavior.ClientCascade)
            //   .IsRequired(false);
        }
    }
}
