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

            entityBuilder.Property(e => e.EmployeeId);
            entityBuilder.HasIndex(e => e.EmployeeId);
            entityBuilder.Property(e => e.DepartmentId);
            entityBuilder.HasIndex(e => e.DepartmentId);

        }
    }
}
