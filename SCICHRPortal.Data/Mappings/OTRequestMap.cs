using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities;

namespace SCICHRPortal.Data.Mappings
{
    public class OTRequestMap
    {
        public OTRequestMap(EntityTypeBuilder<OTRequest> entityBuilder)
        {
            entityBuilder.HasKey(e => e.OTRequestId);
            entityBuilder.Property(e => e.DateIn).IsRequired();
            entityBuilder.Property(e => e.DateOut).IsRequired();
            entityBuilder.Property(e => e.TimeIn).IsRequired();
            entityBuilder.Property(e => e.TimeOut).IsRequired();
            entityBuilder.Property(e => e.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(e => e.UpdatedBy).HasMaxLength(120);
            entityBuilder.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entityBuilder.HasIndex(e => new { e.EmployeeId, e.DateIn, e.DateOut});
        }
    }
}
