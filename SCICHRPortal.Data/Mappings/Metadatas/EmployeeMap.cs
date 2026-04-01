using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Data.Mappings.Metadatas
{
    public class EmployeeMap
    {
        public EmployeeMap(EntityTypeBuilder<Employee> entityBuilder)
        {
            entityBuilder.HasKey(t => t.EmployeeId);
            entityBuilder.Property(t => t.DepartmentId).IsRequired(false);
            entityBuilder.Property(t => t.PositionId).IsRequired(false);
            entityBuilder.Property(t => t.EmployeeNo).HasMaxLength(20).IsRequired();
            entityBuilder.Property(t => t.LastName).HasMaxLength(30).IsRequired();
            entityBuilder.Property(t => t.FirstName).HasMaxLength(30).IsRequired();
            entityBuilder.Property(t => t.MiddleName).HasMaxLength(30).IsRequired();
            entityBuilder.Property(t => t.Suffix).HasMaxLength(10).IsRequired();
            entityBuilder.Property(t => t.Address).HasMaxLength(2000).IsRequired(false);
            entityBuilder.Property(t => t.Email).HasMaxLength(100).IsRequired(true);
            entityBuilder.Property(t => t.ContactNumber).HasMaxLength(100).IsRequired(true);
            entityBuilder.Property(t => t.CreatedBy).HasMaxLength(120);
            entityBuilder.Property(t => t.UpdatedBy).HasMaxLength(120);

            entityBuilder.HasOne(e => e.Department)
               .WithMany()
               .HasForeignKey(u => u.DepartmentId)
               .OnDelete(DeleteBehavior.NoAction);
            entityBuilder.HasOne(e => e.Position)
               .WithMany()
               .HasForeignKey(u => u.PositionId)
               .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
