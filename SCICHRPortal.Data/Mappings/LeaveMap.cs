using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class LeaveMap
    {
        public LeaveMap(EntityTypeBuilder<Leave> entityBuilder)
        {
            entityBuilder.HasKey(e => e.LeaveId);
            entityBuilder.Property(e => e.From).IsRequired();
            entityBuilder.Property(e => e.To).IsRequired();
            entityBuilder.Property(e => e.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(e => e.UpdatedBy).HasMaxLength(120);
            entityBuilder.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entityBuilder.HasOne(e => e.LeaveType).WithMany().HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            entityBuilder.HasIndex(e => new { e.EmployeeId, e.From, e.To });
        }
    }
}
